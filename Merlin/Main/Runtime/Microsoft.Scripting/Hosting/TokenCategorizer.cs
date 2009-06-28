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
using System.Dynamic;
using System.Security.Permissions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    public sealed class TokenCategorizer
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
    {
        private readonly TokenizerService _tokenizer;

        internal TokenCategorizer(TokenizerService tokenizer) {
            Assert.NotNull(tokenizer);
            _tokenizer = tokenizer;
        }

        public void Initialize(object state, ScriptSource scriptSource, SourceLocation initialLocation) {
            _tokenizer.Initialize(state, scriptSource.SourceUnit.GetReader(), scriptSource.SourceUnit, initialLocation);
        }

        /// <summary>
        /// The current internal state of the scanner.
        /// </summary>
        public object CurrentState {
            get { return _tokenizer.CurrentState; }
        }

        /// <summary>
        /// The current startLocation of the scanner.
        /// </summary>
        public SourceLocation CurrentPosition {
            get { return _tokenizer.CurrentPosition; }
        }

        /// <summary>
        /// Move the tokenizer past the next token and return its category.
        /// </summary>
        /// <returns>The token information associated with the token just scanned.</returns>
        public TokenInfo ReadToken() {
            return _tokenizer.ReadToken();
        }

        public bool IsRestartable {
            get { return _tokenizer.IsRestartable; }
        }

        // TODO: Should be ErrorListener
        public ErrorSink ErrorSink {
            get { return _tokenizer.ErrorSink; }
            set { _tokenizer.ErrorSink = value; }
        }

        /// <summary>
        /// Move the tokenizer past the next token.
        /// </summary>
        /// <returns><c>False</c> if the end of stream has been reached, <c>true</c> otherwise.</returns>
        public bool SkipToken() {
            return _tokenizer.SkipToken();
        }

        /// <summary>
        /// Get all tokens over a block of the stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The scanner should return full tokens. If startLocation + length lands in the middle of a token, the full token
        /// should be returned.
        /// </para>
        /// </remarks>s
        /// <param name="characterCount">The mininum number of characters to process while getting tokens.</param>
        /// <returns>A enumeration of tokens.</returns>
        public IEnumerable<TokenInfo> ReadTokens(int characterCount) {
            return _tokenizer.ReadTokens(characterCount);
        }

        /// <summary>
        /// Scan from startLocation to at least startLocation + length.
        /// </summary>
        /// <param name="characterCount">The mininum number of characters to process while getting tokens.</param>
        /// <remarks>
        /// This method is used to determine state at arbitrary startLocation.
        /// </remarks>
        /// <returns><c>False</c> if the end of stream has been reached, <c>true</c> otherwise.</returns>
        public bool SkipTokens(int characterCount) {
            return _tokenizer.SkipTokens(characterCount);
        }

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
