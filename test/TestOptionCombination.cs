using FluentAssertions;
using LanguageExt;
using Newtonsoft.Json;
using RZ.Foundation.Extensions;
using RZ.Foundation.NewtonsoftJson;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation
{
    public class TestOptionCombination
    {
        [Fact]
        public void TestSerializeInt() {
            var x = new TestOptionInt {A = 123};

            var s = JsonConvert.SerializeObject(x);

            s.Should().Be(@"{""A"":123}");
        }

        [Fact]
        public void TestDeserializeInt() {
            var x = JsonConvert.DeserializeObject<TestOptionInt>(@"{""A"":123}");

            x.A.IsSome.Should().BeTrue();
            x.A.Get().Should().Be(123);
        }

        [Fact]
        public void TestSerializeIntNone() {
            var x = new TestOptionInt();

            var s = JsonConvert.SerializeObject(x);

            s.Should().Be(@"{""A"":null}");
        }

        [Fact]
        public void TestDeserializeIntNone() {
            var x = JsonConvert.DeserializeObject<TestOptionInt>(@"{""A"":null}");

            x.A.IsNone.Should().BeTrue();
        }
        [Fact]
        public void TestSerializeOptionTimeRange() {
            var x = new TestOptionTimeRange{A = TimeRange.Parse("10:00 - 12:00")};

            var s = JsonConvert.SerializeObject(x, Formatting.None, new TimeRangeJsonConverter());

            s.Should().Be(@"{""A"":""10:00 - 12:00""}");
        }

        [Fact]
        public void TestDeserializeOptionTimeRange() {
            var x = JsonConvert.DeserializeObject<TestOptionTimeRange>(@"{""A"":""10:00 - 12:00""}", new TimeRangeJsonConverter())!;

            x.A.IsSome.Should().BeTrue();
            x.A.Get().Should().Be(TimeRange.Parse("10:00 - 12:00"));
        }
        [Fact]
        public void TestSerializeOptionTimeRangeNone() {
            var x = new TestOptionTimeRange();

            var s = JsonConvert.SerializeObject(x);

            s.Should().Be(@"{""A"":null}");
        }

        [Fact]
        public void TestDeserializeOptionTimeRangeNone() {
            var x = JsonConvert.DeserializeObject<TestOptionTimeRange>(@"{""A"":null}");

            x.A.IsNone.Should().BeTrue();
        }

        sealed class TestOptionInt
        {
            [JsonConverter(typeof(OptionConverter<int>))]
            public Option<int> A;
        }

        sealed class TestOptionTimeRange
        {
            [JsonConverter(typeof(OptionConverter<TimeRange>))]
            public Option<TimeRange> A;
        }
    }
}