// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using Xunit;

namespace System.Net.Http.Unit.Tests
{
    public class RangeConditionParserTest
    {
        [Fact]
        public void Properties_ReadValues_MatchExpectation()
        {
            HttpHeaderParser parser = GenericHeaderParser.RangeConditionParser;
            Assert.False(parser.SupportsMultipleValues);
            Assert.Null(parser.Comparer);
        }

        [Fact]
        public void TryParse_SetOfValidValueStrings_ParsedCorrectly()
        {
            CheckValidParsedValue("X  \"x\" ", 1, new RangeConditionHeaderValue("\"x\""), 7);
            CheckValidParsedValue("  Sun, 06 Nov 1994 08:49:37 GMT ", 0,
                new RangeConditionHeaderValue(new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero)), 32);
        }

        [Fact]
        public void TryParse_SetOfInvalidValueStrings_ReturnsFalse()
        {
            CheckInvalidParsedValue("\"x\" ,", 0); // no delimiter allowed
            CheckInvalidParsedValue("Sun, 06 Nov 1994 08:49:37 GMT ,", 0); // no delimiter allowed
            CheckInvalidParsedValue("\"x\" Sun, 06 Nov 1994 08:49:37 GMT", 0);
            CheckInvalidParsedValue("Sun, 06 Nov 1994 08:49:37 GMT \"x\"", 0);
            CheckInvalidParsedValue(null, 0);
            CheckInvalidParsedValue(string.Empty, 0);
        }

        #region Helper methods

        private void CheckValidParsedValue(string input, int startIndex, RangeConditionHeaderValue expectedResult,
            int expectedIndex)
        {
            HttpHeaderParser parser = GenericHeaderParser.RangeConditionParser;
            object result = null;
            Assert.True(parser.TryParseValue(input, null, ref startIndex, out result),
                string.Format("TryParse returned false. Input: '{0}'", input));
            Assert.Equal(expectedIndex, startIndex);
            Assert.Equal(expectedResult, result);
        }

        private void CheckInvalidParsedValue(string input, int startIndex)
        {
            HttpHeaderParser parser = GenericHeaderParser.RangeConditionParser;
            object result = null;
            int newIndex = startIndex;
            Assert.False(parser.TryParseValue(input, null, ref newIndex, out result),
                string.Format("TryParse returned true. Input: '{0}'", input));
            Assert.Equal(null, result);
            Assert.Equal(startIndex, newIndex);
        }
        #endregion
    }
}
