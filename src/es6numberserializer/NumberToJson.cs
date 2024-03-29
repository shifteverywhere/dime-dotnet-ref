/*
 *  Copyright 2006-2018 WebPKI.org (http://webpki.org).
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

// Ported to C# from the Mozilla "Rhino" project by Anders Rundgren.
// Some clean up and C#10/.NET 6 updates made by Shift Everywhere

using System.Text;

namespace es6numberserializer;

/// <summary>
/// This is the only public class of the ES6 compatible JSON Number serializer.
/// </summary>
public static class NumberToJson
{
    /*
     * Formats a JSON number according to ES6.
     * This code is emulating 7.1.12.1 of the EcmaScript V6 specification.
     */
    public static string SerializeNumber(double value)
    {
        // 1. Check for JSON compatibility.
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("NaN/Infinity are not permitted in JSON");

        // 2.Deal with zero separately.  Note that this test takes "-0.0" as well
        if (value == 0.0)
            return "0";

        // 3. Call the DtoA algorithm crunchers
        // V8 FastDtoa can't convert all numbers, so try it first but
        // fall back to old DToA in case it fails
        var result = NumberFastDToA.NumberToString(value);
        if (result != null)
            return result;

        var buffer = new StringBuilder();
        NumberDToA.JS_dtostr(buffer, NumberDToA.DtostrStandard, 0, value);
        return buffer.ToString();
    }
}