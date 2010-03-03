/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Math; 
#else
using System.Numerics;
using Microsoft.Scripting.Utils;
#endif

namespace IronPythonTest {
    public static class System_Scripting_Math {
        public static BigInteger CreateBigInteger(int sign, params uint[] data) {
#if CLR2
            return new BigInteger(sign, data);
#else
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign != 0, "sign");

            byte[] dataBytes = new byte[data.Length * 4 + 1];
            for (int i = 0; i < data.Length; i++) {
                uint datum = data[i];
                for (int j = 0; j < 4; j++) {
                    dataBytes[i * 4 + j] = (byte)(datum & 0xff);
                    datum <<= 8;
                }
            }
            BigInteger res = new BigInteger(dataBytes);
            return sign < 0 ? -res : res;
#endif
        }
    }
}
