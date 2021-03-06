﻿using FluentAssertions;
using Newtonsoft.Json;
using RZ.Foundation.NewtonsoftJson;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation
{
    public class TestTimeRange
    {
        [Fact]
        public void TestSerializer() {
            var x = new Test {A = TimeRange.Parse("10:30 - 12:30")};

            var s = JsonConvert.SerializeObject(x, Formatting.None, new TimeRangeJsonConverter());

            s.Should().Be(@"{""A"":""10:30 - 12:30""}");
        }

        [Fact]
        public void TestDeserializer() {
            var x = JsonConvert.DeserializeObject<Test>(@"{""A"":""10:30 - 12:30""}", new TimeRangeJsonConverter())!;

            x.A.Should().Be(TimeRange.Parse("10:30 - 12:30"));
        }

        sealed class Test
        {
            public TimeRange A;
        }
    }
}