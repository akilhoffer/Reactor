using Reactor.ScheduleStream.Sparks;
using Reactor.ServiceGrid;
using Samurai.Wakizashi.Extensions;

namespace Reactor.UtilityConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateSerializedHourlySpark();
        }

        private static void CreateSerializedHourlySpark()
        {
            var spark = new HourlySpark
                            {
                                IsActive = true,
                                Name = "TestSpark",
                                ReactionIdentifier = new ServiceIdentifier("TestReaction", "1.0.0.0")
                            };

            var serializedSpark = new SerializedSpark
                                      {
                                          FullyQualifiedSparkType = spark.GetType().AssemblyQualifiedName,
                                          SparkInstanceXml = spark.ToXml()
                                      };

            var xml = serializedSpark.ToXml();
        }
    }
}
