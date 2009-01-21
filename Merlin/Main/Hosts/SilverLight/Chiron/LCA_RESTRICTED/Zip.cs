/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Chiron {

    /// <summary>
    /// ZipArchive represents a Zip Archive.  It uses the System.IO.File structure as its guide 
    /// 
    /// The largest structual difference between a ZipArchive and the textStream system is that the archive has no
    /// independent notion of a 'directory'.  Instead files know their complete path name.  For the most
    /// part this difference is hard to notice, but does have some ramifications.  For example there is no
    /// concept of the modification time for a directory.    
    /// 
    /// TODO: Opening a textStream for Read/Write without truncation. 
    /// TODO: Allowing different text encodings
    /// </summary>
    internal sealed class ZipArchive {
        // These are the primitive operations  

        /// <summary>
        /// Openes an existing ZIP archive 'archivePath' for reading.  
        /// </summary>
        /// <param name="archivePath"></param>
        public ZipArchive(string archivePath) : this(archivePath, FileAccess.Read) { }
        /// <summary>
        /// Opens a ZIP archive, 'archivePath'  If 'access' is ReadWrite or Write then the target 
        /// does not need to exist, but will be created with the ZipArchive is closed.  
        /// 
        /// If 'access' is ReadWrite the target can exist, and that data is used to initially
        /// populate the archive.  Any modifications that were made will be updated when the
        /// Close() method is called (and not before).  
        /// 
        /// If 'access' is Write then the target is either created or truncated to 0 before 
        /// the archive is written (thus the original data in the archiveFile is ignored).  
        /// </summary>
        public ZipArchive(string archivePath, FileAccess access) {
            entries = new SortedDictionary<string, ZipArchiveFile>(StringComparer.OrdinalIgnoreCase);
            this.archivePath = archivePath;
            this.access = access;
            if (access == FileAccess.Read)
                fromStream = new FileStream(archivePath, FileMode.Open, access);
            else if (access == FileAccess.ReadWrite)
                fromStream = new FileStream(archivePath, FileMode.OpenOrCreate, access);

            // For the write case, we are lazy so as not to empty files on failure. 

            if (fromStream != null)
                Read(fromStream);
        }

        /// <summary>
        /// Read an archive from an exiting stream or write a new archive into a stream
        /// </summary>
        public ZipArchive(Stream fromStream, FileAccess desiredAccess) {
            entries = new SortedDictionary<string, ZipArchiveFile>(StringComparer.OrdinalIgnoreCase);

            this.access = desiredAccess;
            this.fromStream = fromStream;

            if ((desiredAccess & FileAccess.Read) != 0) {
                if (!fromStream.CanRead)
                    throw new Exception("Error: Can't read from stream.");
                Read(fromStream);
            } else if ((desiredAccess & FileAccess.Write) != 0) {
                if (!fromStream.CanWrite)
                    throw new Exception("Error: Can't write to stream.");
            }
        }

        /// <summary>
        /// Enumerate the files in the archive (directories don't have an independent existance).
        /// </summary>
        public IEnumerable<ZipArchiveFile> Files {
            get {
                return entries.Values;
            }
        }
        /// <summary>
        /// Returns a subset of the files in the archive that are in the directory 'archivePath'.  If
        /// searchOptions is TopDirectoryOnly only files in the directory 'archivePath' are returns. 
        /// If searchOptions is AllDirectories then all files that are in subdiretories are also returned. 
        /// </summary>
        public IEnumerable<ZipArchiveFile> GetFilesInDirectory(string archivePath, SearchOption searchOptions) {
            foreach (ZipArchiveFile entry in entries.Values) {
                string name = entry.Name;
                if (name.StartsWith(archivePath, StringComparison.OrdinalIgnoreCase) && name.Length > archivePath.Length) {
                    if (searchOptions == SearchOption.TopDirectoryOnly) {
                        if (name.IndexOf(Path.DirectorySeparatorChar, archivePath.Length + 1) >= 0)
                            continue;
                    }
                    yield return entry;
                }
            }
        }

        /// <summary>
        /// Fetch a archiveFile by name.  'archivePath' is the full path name of the archiveFile in the archive.  
        /// It returns null if the name does not exist (and e
        /// </summary>
        public ZipArchiveFile this[string archivePath] {
            get {
                ZipArchiveFile ret = null;
                entries.TryGetValue(archivePath, out ret);
                return ret;
            }
        }

        /// <summary>
        /// Open the archive textStream 'archivePath' for reading and returns the resulting Stream.
        /// KeyNotFoundException is thrown if 'archivePath' does not exist
        /// </summary>
        public Stream OpenRead(string archivePath) {
            return entries[archivePath].OpenRead();
        }
        /// <summary>
        /// Opens the archive textStream 'archivePath' for writing and returns the resulting Stream. If the textStream
        /// already exists, it is truncated to be an empty textStream.
        /// </summary>
        public Stream Create(string archivePath) {
            ZipArchiveFile newEntry;
            if (!entries.TryGetValue(archivePath, out newEntry))
                newEntry = new ZipArchiveFile(this, archivePath);
            return newEntry.Create();
        }

        /// <summary>
        /// Returns true if the archive can not be written to (it was opend with FileAccess.Read). 
        /// </summary>
        public bool IsReadOnly {
            get { return access == FileAccess.Read; }
            set {
                if (fromStream != null) {
                    if (value == true) {
                        if (!fromStream.CanRead)
                            throw new Exception("Can't read from stream");
                        access = FileAccess.Read;
                    } else {
                        if (fromStream.CanWrite == false)
                            throw new ArgumentException("Can't reset IsReadOnly on a ZipArchive whose stream is ReadOnly.");
                        access = (fromStream.CanRead) ? FileAccess.ReadWrite : FileAccess.Write;
                    }
                } else {
                    access = value ? FileAccess.Read : FileAccess.ReadWrite;
                }
            }
        }
        /// <summary>
        /// Closes the archive.  Until this call is made any pending modifications to the archive are NOT
        /// made (the archive is unchanged).  
        /// </summary>
        public void Close() {
            closeCalled = true;
            if (!IsReadOnly) {
                if (fromStream == null) {
                    Debug.Assert(archivePath != null);
                    Debug.Assert(access == FileAccess.Write);
                    fromStream = new FileStream(archivePath, FileMode.Create);
                }

                fromStream.Position = 0;
                fromStream.SetLength(0);      // delete the data in the stream.

                foreach (ZipArchiveFile entry in entries.Values) {
                    entry.WriteToStream(fromStream);
                }

                WriteArchiveDirectoryToStream(fromStream);
            }
            fromStream.Close();
        }
        /// <summary>
        /// Remove all files from the archive. 
        /// </summary>
        public void Clear() {
            entries.Clear();
        }
        /// <summary>
        /// Count of total number of files (does not include directories) in the archive. 
        /// </summary>
        public int Count { get { return entries.Count; } }

        // These are convinence methods (could be implemented outside this class)

        /// <summary>
        /// Returns true if 'archivePath' exists in the archive.  
        /// </summary>
        /// <returns></returns>
        public bool Exists(string archivePath) {
            return entries.ContainsKey(archivePath);
        }
        /// <summary>
        ///  Renames sourceArchivePath to destinationArchivePath.  If destinationArchivePath exists it is
        ///  discarded.  
        /// </summary>
        public void Move(string sourceArchivePath, string destinationArchivePath) {
            entries[sourceArchivePath].MoveTo(destinationArchivePath);
        }
        /// <summary>
        /// Delete 'archivePath'.  It returns true if successful.  If archivePath does not exist, it
        /// simply returns false (no exception is thrown).  The delete succeeds even if streams on the
        /// data exists (they continue to exist, but will not be persisted on Close()
        /// </summary>
        public bool Delete(string archivePath) {
            ZipArchiveFile entry;
            if (!entries.TryGetValue(archivePath, out entry))
                return false;
            entry.Delete();
            return true;
        }
        /// <summary>
        /// Copies the archive textStream 'sourceArchivePath' to the textStream system textStream 'targetFilePath'. 
        /// It will overwrite existing files, however a locked targetFilePath will cause an exception.  
        /// </summary>
        /// <param name="sourceArchivePath"></param>
        /// <param name="targetFilePath"></param>
        public void CopyToFile(string sourceArchivePath, string targetFilePath) {
            entries[sourceArchivePath].CopyToFile(targetFilePath);
        }
        /// <summary>
        /// Copyies 'sourceFilePath from the textStream system to the archive as 'targetArchivePath'
        /// It will overwrite any existing textStream.
        /// </summary>
        public void CopyFromFile(string sourceFilePath, string targetArchivePath) {
            using (Stream inFile = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            using (Stream outFile = Create(targetArchivePath))
                ZipArchiveFile.CopyStream(inFile, outFile);
            this[targetArchivePath].LastWriteTime = File.GetLastWriteTime(sourceFilePath);
        }
        /// <summary>
        /// Deletes all files in the directory (and subdirectories) of 'archivePath'.  
        /// </summary>
        public int DeleteDirectory(string archivePath) {
            int ret = 0;
            List<ZipArchiveFile> entriesToDelete = new List<ZipArchiveFile>(GetFilesInDirectory(archivePath, SearchOption.AllDirectories));
            foreach (ZipArchiveFile entry in entriesToDelete) {
                entry.Delete();
                ret++;
            }
            return ret;
        }
        /// <summary>
        /// Copies (recursively the files in archive directory to a textStream system directory.
        /// </summary>
        /// <param name="sourceArchiveDirectory">The name of the source directory in the archive</param>
        /// <param name="targetDirectory">The target directory in the textStream system to copy to. 
        /// If it is empty it represents all files in the archive. </param>
        public void CopyToDirectory(string sourceArchiveDirectory, string targetDirectory) {
            foreach (ZipArchiveFile entry in GetFilesInDirectory(sourceArchiveDirectory, SearchOption.AllDirectories)) {
                string relativePath = GetRelativePath(entry.Name, sourceArchiveDirectory);
                entry.CopyToFile(Path.Combine(targetDirectory, relativePath));
            }
        }
        /// <summary>
        /// Copies a directory recursively from the textStream system to the archive.  
        /// </summary>
        /// <param name="sourceDirectory">The direcotry in the textStream system to copy to the archive</param>
        /// <param name="targetArchiveDirectory">
        /// The directory in the archive to copy to.  An empty string means the top level of the archive</param>
        public void CopyFromDirectory(string sourceDirectory, string targetArchiveDirectory) {
            foreach (string path in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories)) {
                string relativePath = GetRelativePath(path, sourceDirectory);
                CopyFromFile(path, Path.Combine(targetArchiveDirectory, relativePath));
            }
        }

        /// <summary>
        /// Open an existing textStream in the archive for reading as text and returns the resulting StreamReader.  
        /// </summary>
        public StreamReader OpenText(string archivePath) {
            return entries[archivePath].OpenText();
        }
        /// <summary>
        /// Opens a textStream in the archive for writing as a text textStream.  Returns the resulting TextWriter.  
        /// </summary>
        public TextWriter CreateText(string archivePath) {
            ZipArchiveFile newEntry;
            if (!entries.TryGetValue(archivePath, out newEntry))
                newEntry = new ZipArchiveFile(this, archivePath);
            return newEntry.CreateText();
        }

        /// <summary>
        /// Reads all the data in 'archivePath' as a text string and returns it. 
        /// </summary>
        public string ReadAllText(string archivePath) {
            return entries[archivePath].ReadAllText();
        }
        /// <summary>
        /// Overwrites the archive textStream 'archivePath' with the text in 'data'
        /// </summary>
        public void WriteAllText(string archivePath, string data) {
            ZipArchiveFile newEntry;
            if (!entries.TryGetValue(archivePath, out newEntry))
                newEntry = new ZipArchiveFile(this, archivePath);
            newEntry.WriteAllText(data);
        }

        /// <summary>
        /// Returns a string reprentation of the archive (its name if known, and count of files)
        /// Mostly useful in the debugger.  
        /// </summary>
        public override string ToString() {
            string name = archivePath;
            if (archivePath == null)
                name = "<fromStream>";

            return "ZipArchive " + name + " FileCount = " + entries.Count;
        }

        #region PrivateState
        internal SortedDictionary<string, ZipArchiveFile> entries;
        internal Stream fromStream;

        private FileAccess access;
        private string archivePath;
        private bool closeCalled;
        #endregion

        #region PrivateImplementation

        ~ZipArchive() {
            Debug.Assert(access == FileAccess.Read || closeCalled || entries.Count == 0,
                "Did not close a writable archive (use clear to abandon it)");
        }

        // This is really a general purpose routine, but I put it here to avoid taking a dependency. 
        internal static string GetRelativePath(string fileName, string directory) {
            Debug.Assert(fileName.StartsWith(directory), "directory not a prefix");

            int directoryEnd = directory.Length;
            if (directoryEnd == 0)
                return fileName;
            while (directoryEnd < fileName.Length && fileName[directoryEnd] == Path.DirectorySeparatorChar)
                directoryEnd++;
            string relativePath = fileName.Substring(directoryEnd);
            return relativePath;
        }


        private void Read(Stream archiveStream) {
            // TODO for seekable streams, seek to the end of the stream, and use the archive directory there 
            // to avoid reading most of the file.  
            for (; ; ) {
                ZipArchiveFile entry = ZipArchiveFile.Read(this);
                if (entry == null)
                    break;
            }
        }
        private void WriteArchiveDirectoryToStream(Stream writer) {
            // Write the directory entries.  
            long startOfDirectory = writer.Position;
            foreach (ZipArchiveFile entry in entries.Values)
                entry.WriteArchiveDirectoryEntryToStream(writer);
            long endOfDirectory = writer.Position;

            // Write the trailer
            ByteBuffer trailer = new ByteBuffer(22);
            trailer.WriteUInt32(ZipArchiveFile.SignatureArchiveDirectoryEnd);
            trailer.WriteUInt16(ZipArchiveFile.DiskNumberStart);
            trailer.WriteUInt16(ZipArchiveFile.DiskNumberStart);
            trailer.WriteUInt16((ushort)entries.Count);
            trailer.WriteUInt16((ushort)entries.Count);
            trailer.WriteUInt32((uint)(endOfDirectory - startOfDirectory));      // directory size
            trailer.WriteUInt32((uint)startOfDirectory);                         // directory start
            trailer.WriteUInt16((ushort)ZipArchiveFile.FileCommentLength);
            trailer.WriteContentsTo(writer);
        }
        #endregion
    }

    /// <summary>
    /// ZipArchiveFile represents one archiveFile in the ZipArchive.   It is analogous to the System.IO.DiffFile
    /// object for normal files.  
    /// </summary>
    public sealed class ZipArchiveFile {
        // These are the primitive operations  

        /// <summary>
        /// Truncates the archiveFile represented by the ZipArchiveFile to be empty and returns a Stream that can be used
        /// to write (binary) data into it.
        /// </summary>
        /// <returns>A Stream that can be written on. </returns>
        public Stream Create() {
            if (IsReadOnly)
                throw new ApplicationException("Archive is ReadOnly");
            if (uncompressedData != null && (uncompressedData.CanWrite || uncompressedData.CanRead))
                throw new ApplicationException("ZipArchiveFile already open.");

            // abandon any old data
            compressedData = null;
            positionOfCompressedDataInArchive = 0;
            compressedLength = 0;

            // We allocate some buffer so that GetBuffer() does not return null. 
            uncompressedData = new RepairedMemoryStream(256);
            return uncompressedData;
        }
        /// <summary>
        /// Opens the archiveFile represented by the ZipArchiveFile and returns a stream that can use to read (binary) data.
        /// </summary>
        /// <returns>A Stream that can be read from.</returns>
        public Stream OpenRead() {
            if (uncompressedData == null) {
                if (compressedData == null) {
                    // TODO if we had a rangeStream, we could avoid this copy. 
                    compressedData = new byte[compressedLength];
                    archive.fromStream.Seek(positionOfCompressedDataInArchive, SeekOrigin.Begin);
                    archive.fromStream.Read(compressedData, 0, compressedLength);
                }
                MemoryStream compressedReader = new MemoryStream(compressedData);
                if (compressionMethod == CompressionMethod.None)
                    return compressedReader;
                else
                    return new DeflateStream(compressedReader, CompressionMode.Decompress);
            } else {
                if (uncompressedData.CanWrite)
                    throw new ApplicationException("ZipArchiveFile still open for writing.");
                return new MemoryStream(uncompressedData.GetBuffer(), 0, (int)uncompressedData.Length, false);
            }
        }
        /// <summary>
        /// Truncates the archiveFile represented by the ZipArchiveFile to be empty and returns a TextWriter that text
        /// can be written to (using the default encoding). 
        /// </summary>
        /// <returns>The TextWriter that text can be written to. </returns>
        public void MoveTo(string newArchivePath) {
            if (IsReadOnly)
                throw new ApplicationException("Archive is ReadOnly");

            archive.entries.Remove(name);
            name = newArchivePath;
            archive.entries[newArchivePath] = this;
        }
        /// <summary>
        /// Delete the archiveFile represented by the ZipArchiveFile.   The textStream can be in use without conflict.
        /// Deleting a textStream simply means it will not be persisted when ZipArchive.Close() is called.  
        /// </summary>
        public void Delete() {
            if (IsReadOnly)
                throw new ApplicationException("Archive is ReadOnly");

            archive.entries.Remove(name);
            name = null;
            archive = null;
            uncompressedData = null;
            compressedData = null;
        }

        // Properties. 
        /// <summary>
        /// The last time the archive was updated (Create() was called).   The copy operations transfer the
        /// LastWriteTime from the source to the target.  
        /// </summary>
        public DateTime LastWriteTime {
            get { return lastWriteTime; }
            set {
                if (IsReadOnly)
                    throw new ApplicationException("Archive is ReadOnly");
                lastWriteTime = value;
            }
        }
        /// <summary>
        /// The length of the archive textStream in bytes. 
        /// </summary>
        public long Length {
            get {
                if (uncompressedData != null)
                    return uncompressedData.Length;
                else
                    return length;
            }
        }
        /// <summary>
        /// The name in the archive. 
        /// </summary>
        public string Name {
            get { return name; }
            set { MoveTo(value); }
        }
        /// <summary>
        /// The CRC32 checksum associated with the data.  Useful for quickly determining if the data has
        /// changed.  
        /// </summary>
        public uint CheckSum {
            get {
                if (crc32 == null) {
                    Debug.Assert(uncompressedData != null);
                    crc32 = Crc32.Calculate(0, uncompressedData.GetBuffer(), 0, (int)uncompressedData.Length);
                }
                return crc32.Value;
            }
        }
        /// <summary>
        /// The archive assoated with the ZipArchiveFile. 
        /// </summary>
        internal ZipArchive Archive { get { return archive; } }
        /// <summary>
        /// Returns true if the textStream can's be written (the archive is read-only.  
        /// </summary>
        public bool IsReadOnly { get { return archive.IsReadOnly; } }

        // Helpful for debugging since VS displays properties by default. 
#if DEBUG
        public string DataAsText { get { return ReadAllText(); } }
#endif

        /// <summary>
        ///  A text summary of the archive textStream (its name and length).  
        /// </summary>
        public override string ToString() {
            return "ZipArchiveEntry " + Name + " length " + Length;
        }

        // These are convinence methods (could be implemented outside this class)

        /// <summary>
        /// Truncate the archive textStream and return a StreamWrite sutable for writing text to the textStream. 
        /// </summary>
        /// <returns></returns>
        public StreamWriter CreateText() {
            return new StreamWriter(Create());
        }
        /// <summary>
        /// Opens the archiveFile represented by the ZipArchiveFile and returns a stream that can use to read text.
        /// </summary>
        /// <returns>A TextReader text can be read from.</returns>
        public StreamReader OpenText() {
            return new StreamReader(OpenRead());
        }
        /// <summary>
        /// Read all the text from the archiveFile represented by the ZipArchiveFile and return it as a string. 
        /// </summary>
        /// <returns>The string contained in the archiveFile</returns>
        public string ReadAllText() {
            TextReader reader = OpenText();
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }
        /// <summary>
        /// Replaces the data in the archiveFile represented by the ZipArchiveFile with the text in 'data'
        /// </summary>
        /// <param name="data">The data to replace the archiveFile data with.</param>
        public void WriteAllText(string data) {
            TextWriter writer = CreateText();
            writer.Write(data);
            writer.Close();
        }
        /// <summary>
        /// Copy the data in from the 'this' ZipArchiveFile to the archive textStream named 'outputFilePath' in
        /// to the file system at 'outputFilePath' 
        /// </summary>
        public void CopyToFile(string outputFilePath) {
            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (outputDirectory.Length > 0)
                Directory.CreateDirectory(outputDirectory);
            using (Stream outFile = new FileStream(outputFilePath, FileMode.Create))
            using (Stream inFile = OpenRead())
                CopyStream(inFile, outFile);
            File.SetLastWriteTime(outputFilePath, LastWriteTime);
        }
        /// <summary>
        /// Copy the data in archive textStream named 'inputFilePath' into the 'this' archive textStream.  (discarding
        /// what was there before). 
        /// </summary>
        public void CopyTo(string outputArchivePath) {
            using (Stream outFile = archive.Create(outputArchivePath))
            using (Stream inFile = OpenRead())
                CopyStream(inFile, outFile);
            archive[outputArchivePath].LastWriteTime = LastWriteTime;
        }

        #region PrivateState
        private uint? crc32;
        private string name;
        private DateTime lastWriteTime;
        private long length;
        private int compressedLength;
        private CompressionMethod compressionMethod;
        private ZipArchive archive;                // The archive this entry belongs to. 
        private uint headerOffset; // the location of the header

        // To optimize for the common useage of read-only sequential access of the ZipArchive files we
        // don't uncompress the data immediately. We instead just remember where in the stream the 
        // compressed data lives (positionOfCompressedDataInArhive).  If the archive is read-write, 
        // we can't do this (because we are changing the archive), so in that case we copy the data to
        // 'compressedData'.  If the archiveFile actually gets writen too, however we throw that away and 
        // store the data in uncompressed form.  
        private MemoryStream uncompressedData;
        private byte[] compressedData;             // if uncompressed data is null, look here for compressed data

        // if uncompressed data AND compressed data is null, the data is at this offset in the archive
        // stream.
        private long positionOfCompressedDataInArchive;
        #endregion

        #region PrivateImplementation
        internal const uint SignatureFileEntry = 0x04034b50;
        internal const uint SignatureArchiveDirectory = 0x02014b50;
        internal const uint SignatureArchiveDirectoryEnd = 0x06054b50;
        internal const ushort VersionNeededToExtract = 0x0100; // version 1.0, TODO
        internal const ushort MaximumVersionExtractable = 0x0100;
        internal const ushort VersionMadeBy = 0;               // MS-DOS, TODO: should this be NTFS?
        internal const ushort GeneralPurposeBitFlag = 0;       // TODO
        internal const ushort ExtraFieldLength = 0;
        internal const ushort FileCommentLength = 0;
        internal const ushort DiskNumberStart = 0;
        internal const ushort InternalFileAttributes = 0;      // binary file, TODO: support ASCII?
        internal const uint ExternalFileAttributes = 0;      // TODO: do we want to support?

        internal enum CompressionMethod : ushort {
            None = 0,
            Deflate = 8,
        };

        static char[] invalidPathChars = Path.GetInvalidPathChars();

        static internal int CopyStream(Stream fromStream, Stream toStream) {
            byte[] buffer = new byte[8192];
            int totalBytes = 0;
            for (; ; ) {
                int count = fromStream.Read(buffer, 0, buffer.Length);
                if (count == 0)
                    break;
                toStream.Write(buffer, 0, count);
                totalBytes += count;
            }
            return totalBytes;
        }

        // To safe space, we encode the type as a 32 bit number.  This format is used on DOS
        static private DateTime DosTimeToDateTime(uint dateTime) {
            int dateTimeSigned = (int)dateTime;
            int year = 1980 + (dateTimeSigned >> 25);
            int month = (dateTimeSigned >> 21) & 0xF;
            int day = (dateTimeSigned >> 16) & 0x1F;
            int hour = (dateTimeSigned >> 11) & 0x1F;
            int minute = (dateTimeSigned >> 5) & 0x3F;
            int second = (dateTimeSigned & 0x001F) * 2;       // only 5 bits for second, so we only have a granularity of 2 sec. 
            if (second >= 60)
                second = 0;

            DateTime ret = new DateTime();
            try {
                ret = new System.DateTime(year, month, day, hour, minute, second, 0);
            } catch { }
            return ret;
        }

        // To safe space, we encode the type as a 32 bit number.  This format is used in DOS
        static private uint DateTimeToDosTime(DateTime dateTime) {
            int ret = ((dateTime.Year - 1980) & 0x7F);
            ret = (ret << 4) + dateTime.Month;
            ret = (ret << 5) + dateTime.Day;
            ret = (ret << 5) + dateTime.Hour;
            ret = (ret << 6) + dateTime.Minute;
            ret = (ret << 5) + (dateTime.Second / 2);   // only 5 bits for second, so we only have a granularity of 2 sec. 
            return (uint)ret;
        }

        // These routines are only to be used by ZipArchive.
        // 
        /// <summary>
        /// Used by ZipArchive to write the entry to the archive. 
        /// </summary>
        /// <param name="writer">The stream representing the archive to write the entry to.</param>
        internal void WriteToStream(Stream writer) {
            Debug.Assert(!IsReadOnly);
            Debug.Assert(positionOfCompressedDataInArchive == 0);   // we don't use this on read-write archives. 

            if (uncompressedData != null) {
                if (uncompressedData.CanWrite)
                    throw new Exception("Unclosed writable handle to " + Name + " still exists at Save time");

                // TODO: Consider using seeks instead of copying to the compressed data stream.  
                // TODO support not running Deflate but simply skipping (useful for arcping arc files)
                // 
                // Compress the data
                MemoryStream compressedDataStream = new RepairedMemoryStream((int)(uncompressedData.Length * 5 / 8));
                Stream compressor = new DeflateStream(compressedDataStream, CompressionMode.Compress);
                compressor.Write(uncompressedData.GetBuffer(), 0, (int)uncompressedData.Length);
                compressor.Close();

                // TODO support the NONE case too.  
                compressionMethod = CompressionMethod.Deflate;

                compressedLength = (int)compressedDataStream.Length;
                compressedData = compressedDataStream.GetBuffer();
            }

            Debug.Assert(compressedData != null);
            WriteZipFileHeader(writer);                             // Write the header.
            writer.Write(compressedData, 0, compressedLength);      // Write the data. 
        }

        private void WriteZipFileHeader(Stream writer) {
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(name.Replace(Path.DirectorySeparatorChar, '/'));
            if ((uint)length != length)
                throw new ApplicationException("File length too long.");

            //Local file header:
            //
            //local file header signature     4 bytes  (0x04034b50)
            //version needed to extract       2 bytes
            //general purpose bit flag        2 bytes
            //compression method              2 bytes
            //last mod file time              2 bytes
            //last mod file date              2 bytes
            //crc-32                          4 bytes
            //compressed size                 4 bytes
            //uncompressed size               4 bytes
            //file name length                2 bytes
            //extra field length              2 bytes
            //
            //file name (variable size)
            //extra field (variable size)

            // save the start of the header file for later use in the directory entry
            headerOffset = (uint)writer.Position;

            ByteBuffer header = new ByteBuffer(30);
            header.WriteUInt32(SignatureFileEntry);
            header.WriteUInt16(VersionNeededToExtract);
            header.WriteUInt16(GeneralPurposeBitFlag);
            header.WriteUInt16((ushort)compressionMethod);
            header.WriteUInt32(DateTimeToDosTime(lastWriteTime));
            header.WriteUInt32(CheckSum);
            header.WriteUInt32((uint)compressedLength);
            header.WriteUInt32((uint)Length);
            header.WriteUInt16((ushort)fileNameBytes.Length);
            header.WriteUInt16(ExtraFieldLength);                             // extra field length (unused)

            header.WriteContentsTo(writer);

            writer.Write(fileNameBytes, 0, fileNameBytes.Length);   // Write the archiveFile name. 
        }

        internal void WriteArchiveDirectoryEntryToStream(Stream writer) {
            //File header (in central directory):
            //
            //central file header signature   4 bytes  (0x02014b50)
            //version made by                 2 bytes
            //version needed to extract       2 bytes
            //general purpose bit flag        2 bytes
            //compression method              2 bytes
            //last mod file time              2 bytes
            //last mod file date              2 bytes
            //crc-32                          4 bytes
            //compressed size                 4 bytes
            //uncompressed size               4 bytes
            //file name length                2 bytes
            //extra field length              2 bytes
            //file comment length             2 bytes
            //disk number start               2 bytes
            //internal file attributes        2 bytes
            //external file attributes        4 bytes
            //relative offset of local header 4 bytes
            //
            //file name (variable size)
            //extra field (variable size)
            //file comment (variable size)

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(name);

            ByteBuffer header = new ByteBuffer(46);
            header.WriteUInt32(SignatureArchiveDirectory);
            header.WriteUInt16(VersionMadeBy);
            header.WriteUInt16(VersionNeededToExtract);
            header.WriteUInt16(GeneralPurposeBitFlag);
            header.WriteUInt16((ushort)compressionMethod);
            header.WriteUInt32(DateTimeToDosTime(lastWriteTime));
            header.WriteUInt32(CheckSum);
            header.WriteUInt32((uint)compressedLength);
            header.WriteUInt32((uint)Length);
            header.WriteUInt16((ushort)fileNameBytes.Length);
            header.WriteUInt16(ExtraFieldLength);
            header.WriteUInt16(FileCommentLength);
            header.WriteUInt16(DiskNumberStart);
            header.WriteUInt16(InternalFileAttributes);
            header.WriteUInt32(ExternalFileAttributes);
            header.WriteUInt32(headerOffset);

            header.WriteContentsTo(writer);

            writer.Write(fileNameBytes, 0, fileNameBytes.Length);
        }
        /// <summary>
        /// Create a new archive archiveFile with no data (empty).  It is expected that only ZipArchive methods will
        /// use this routine.  
        /// </summary>
        internal ZipArchiveFile(ZipArchive archive, string archiveName) {
            this.archive = archive;
            name = archiveName;
            if (name != null)
                archive.entries[name] = this;
            lastWriteTime = DateTime.Now;
        }
        /// <summary>
        /// Reads a single archiveFile from a Zip Archive.  Should only be used by ZipArchive.  
        /// </summary>
        /// <returns>A ZipArchiveFile representing the archiveFile read from the archive.</returns>
        internal static ZipArchiveFile Read(ZipArchive archive) {
            Stream reader = archive.fromStream;
            ByteBuffer header = new ByteBuffer(30);
            int count = header.ReadContentsFrom(reader);
            if (count == 0)
                return null;

            //Local file header:
            //
            //local file header signature     4 bytes  (0x04034b50)
            //version needed to extract       2 bytes
            //general purpose bit flag        2 bytes
            //compression method              2 bytes
            //last mod file time              2 bytes
            //last mod file date              2 bytes
            //crc-32                          4 bytes
            //compressed size                 4 bytes
            //uncompressed size               4 bytes
            //file name length                2 bytes
            //extra field length              2 bytes
            //
            //file name (variable size)
            //extra field (variable size)

            uint fileSignature = header.ReadUInt32();
            if (fileSignature != SignatureFileEntry) {
                if (fileSignature != SignatureArchiveDirectory)
                    throw new ApplicationException("Bad ZipFile Header");
                return null;
            }

            ushort versionNeededToExtract = header.ReadUInt16();
            if (versionNeededToExtract > MaximumVersionExtractable)
                throw new ApplicationException("Zip file requires unsupported features");

            header.SkipBytes(2); // general purpose bit flag

            ZipArchiveFile newEntry = new ZipArchiveFile(archive, null);

            newEntry.compressionMethod = (CompressionMethod)header.ReadUInt16();
            newEntry.lastWriteTime = DosTimeToDateTime(header.ReadUInt32());
            newEntry.crc32 = header.ReadUInt32();
            newEntry.compressedLength = checked((int)header.ReadUInt32());
            newEntry.length = header.ReadUInt32();
            int fileNameLength = checked((int)header.ReadUInt16());

            byte[] fileNameBuffer = new byte[fileNameLength];
            int fileNameCount = reader.Read(fileNameBuffer, 0, fileNameLength);
            newEntry.name = Encoding.UTF8.GetString(fileNameBuffer).Replace('/', Path.DirectorySeparatorChar);
            archive.entries[newEntry.name] = newEntry;

            if (count != header.Length || fileNameCount != fileNameLength || fileNameLength == 0 || newEntry.LastWriteTime.Ticks == 0)
                throw new ApplicationException("Bad Zip File Header");
            if (newEntry.Name.IndexOfAny(invalidPathChars) >= 0)
                throw new ApplicationException("Invalid File Name");
            if (newEntry.compressionMethod != CompressionMethod.None && newEntry.compressionMethod != CompressionMethod.Deflate)
                throw new ApplicationException("Unsupported compression mode " + newEntry.compressionMethod);

            if (archive.IsReadOnly && reader.CanSeek) {
                // Optimization: we can defer reading in the data in the common case of a read-only archive.
                // by simply remembering where the data is and fetching it on demand.  This is nice because
                // we only hold on to memory of data we are actually operating on.  (instead of the whole archive)
                // 
                // Because uncompresseData is null, we know that we fetch it from the archive 'fromStream'.
                newEntry.positionOfCompressedDataInArchive = archive.fromStream.Position;
                reader.Seek(newEntry.compressedLength, SeekOrigin.Current);
            } else {
                // We may be updating the archive in place, so we need to copy the data out.  
                newEntry.compressedData = new byte[newEntry.compressedLength];
                reader.Read(newEntry.compressedData, 0, (int)newEntry.compressedLength);
            }

#if DEBUG
            newEntry.Validate();
#endif

            return newEntry;
        }

        internal void Validate() {
            Stream readStream = OpenRead();
            uint crc = 0;
            byte[] buffer = new byte[655536];
            for (; ; ) {
                int count = readStream.Read(buffer, 0, buffer.Length);
                if (count == 0)
                    break;
                crc = Crc32.Calculate(crc, buffer, 0, count);
            }
            readStream.Close();
            if (crc != CheckSum)
                throw new ApplicationException("Error: data checksum failed for " + Name);
        }

        #endregion
    }

    #region helper classes

    /// <summary>
    /// MemoryStream does not let you look at the length after it has been closed.
    /// so we override it here, storing the size when it is closed
    /// </summary>
    internal class RepairedMemoryStream : MemoryStream {
        public RepairedMemoryStream(int size) : base(size) { }
        public override void Close() {
            myLength = Length;
            isClosed = true;
            base.Close();
        }
        public override long Length {
            get {
                return isClosed ? myLength : base.Length;
            }
        }
        long myLength;
        bool isClosed;
    }

    /// <summary>
    /// byte[] array plus current offset.
    /// useful for reading/writing headers, ensuring the offset is updated correctly
    /// </summary>
    internal struct ByteBuffer {
        byte[] buffer;
        int offset;

        public int Length {
            get { return buffer.Length; }
        }

        public ByteBuffer(int size) {
            buffer = new byte[size];
            offset = 0;
        }

        public void SkipBytes(int count) {
            offset += count;
        }

        public uint ReadUInt32() {
            return (uint)(buffer[offset++] | ((buffer[offset++] | ((buffer[offset++] | (buffer[offset++] << 8)) << 8)) << 8));
        }

        public ushort ReadUInt16() {
            return (ushort)(buffer[offset++] | ((buffer[offset++]) << 8));
        }

        public void WriteUInt32(uint value) {
            buffer[offset++] = (byte)value;
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 24);
        }

        public void WriteUInt16(ushort value) {
            buffer[offset++] = (byte)value;
            buffer[offset++] = (byte)(value >> 8);
        }

        public void WriteContentsTo(Stream writer) {
            Debug.Assert(offset == buffer.Length);
            writer.Write(buffer, 0, buffer.Length);
        }

        public int ReadContentsFrom(Stream reader) {
            Debug.Assert(offset == 0);
            return reader.Read(buffer, 0, buffer.Length);
        }
    }
    #endregion

}