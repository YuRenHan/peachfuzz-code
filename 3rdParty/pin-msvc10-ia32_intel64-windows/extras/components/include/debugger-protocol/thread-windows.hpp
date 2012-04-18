/*BEGIN_LEGAL 
Intel Open Source License 

Copyright (c) 2002-2011 Intel Corporation. All rights reserved.
 
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.  Redistributions
in binary form must reproduce the above copyright notice, this list of
conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.  Neither the name of
the Intel Corporation nor the names of its contributors may be used to
endorse or promote products derived from this software without
specific prior written permission.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE INTEL OR
ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
END_LEGAL */
// <ORIGINAL-AUTHOR>: Greg Lueck
// <COMPONENT>: debugger-protocol
// <FILE-TYPE>: component public header

#ifndef DEBUGGER_PROTOCOL_THREAD_WINDOWS_HPP
#define DEBUGGER_PROTOCOL_THREAD_WINDOWS_HPP

namespace DEBUGGER_PROTOCOL {


/*!
 * In the future, new fields may be added to the end of the THREAD_INFO_WINDOWS
 * structure.  If this happens, clients can use the \e _version field to retain
 * backward compatibility.
 *
 * When a client writes information to this structure, it should set \e _version
 * to the latest version that it supports.
 *
 * When a client reads this structure, it should use \e _version to tell which
 * fields are valid.  A client should allow that \e _version may be greater than
 * the newest version it knows about, which happens if an older front-end runs
 * with a newer back-end or vice-versa.
 */
enum THREAD_INFO_WINDOWS_VERSION
{
    THREAD_INFO_WINDOWS_VERSION_0   ///< This is the only defined version currently.
};


/*!
 * Information about a thread running on a Windows target.
 */
struct /*<POD>*/ THREAD_INFO_WINDOWS
{
    THREAD_INFO_WINDOWS_VERSION _version;   ///< Tells which fields in this structure are valid.
    FUND::ANYADDR _teb;                     ///< Address of the thread environment block.

    /*!
     * If non-zero, the thread is suspended by one or more other threads in the target
     * system.  Passing CONTINUE_MODE_FROZEN to ICOMMANDS::SetContinueMode() does not
     * affect the suspension count.
     */
    FUND::UINT64 _suspensionCount;
};

} // namespace
#endif // file guard