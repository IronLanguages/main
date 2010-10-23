/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/
using System;

namespace Microsoft.IronStudio.Repl {
    public interface IReplWindowCommands {

        /// <summary>
        /// Shift+VK_RETURN
        /// </summary>
        void BreakLine();

        /// <summary>
        /// VSConstants.VSStd2KCmdID.CANCEL
        /// </summary>
        void Cancel();

        /// <summary>
        /// VSConstants.VSStd2KCmdID.UP
        /// </summary>
        void SmartUpArrow();

        /// <summary>
        /// VSConstants.VSStd2KCmdID.DOWN
        /// </summary>
        void SmartDownArrow();

        /// </remarks>
        /// <summary>
        /// VSConstants.VSStd2KCmdID.BOL (false)
        /// VSConstants.VSStd2KCmdID.BOL_EXT (true)
        /// </summary>
        /// <remarks>
        /// When on the first line of the current input region but not at the
        /// first position, move to the beginning of the region.  Otherwise, move
        /// to the beginning of the line.
        void Home(bool extendSelection);

        /// <summary>
        /// VSConstants.VSStd97CmdID.Paste
        /// </summary>
        /// <returns></returns>
        bool PasteClipboard();

        /// <summary>
        /// VSConstants.VSStd97CmdID.SelectAll
        /// </summary>
        /// <remarks>
        /// When in any input region, select that region.  If not in an input
        /// region or if selection already corresponds to an input region, select
        /// the entire document
        /// </remarks>
        bool SelectAll();

        /// <summary>
        /// If currently in the active edit executes the command.  Otherwise if we're
        /// in an old input region pastes the current text into the active edit.
        /// </summary>
        void ExecuteOrPasteSelected();

        void HistoryNext();

        void HistoryPrevious();
    }
}
