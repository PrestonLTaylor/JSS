using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JSS.Test262Runner;

/// <summary>
/// Holds the needed parsed metadata of a test-262 test case.
/// </summary>
internal sealed record Test262Metadata
{
    private Test262Metadata(Metadata metadata)
    {
        _metadata = metadata;
    }

    static public Test262Metadata Create(string testCase)
    {
        try
        {
            var yamlString = GetYAMLFromTestCase(testCase);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var metadata = deserializer.Deserialize<Metadata>(yamlString);
            return new Test262Metadata(metadata);
        }
        catch (Exception ex)
        {
            throw new MetadataParsingFailureException(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the YAML metadata from a test-262 test case.
    /// </summary>
    /// <param name="testCase">A test-262 test case that contains YAML metadata.</param>
    /// <returns>A string that holds the YAML metadata of a test case.</returns>
    static private string GetYAMLFromTestCase(string testCase)
    {
        // Each test file may define metadata that describe additional requirements. This information is delimited by the token sequence /*--- and ---*/ and is structured as YAML.
        const int DELIMITER_SIZE = 5;
        var yamlStart = testCase.IndexOf("/*---");
        var yamlEnd = testCase.IndexOf("---*/");

        if (yamlStart == -1 || yamlEnd == -1) throw new MetadataParsingFailureException("Could not find the YAML delimiters for test case.");

        return testCase.Substring(yamlStart + DELIMITER_SIZE, yamlEnd - yamlStart - DELIMITER_SIZE);
    }

    // NOTE: We don't want to expose these classes as we just want the abstracted properties of the outer class
    sealed record Metadata
    {
        public NegativeTestMetadata? Negative { get; set; }
        public List<string>? Includes { get; set; }
        public HashSet<string>? Flags { get; set; }
    }

    enum NegativeTestPhase
    {
        Parse,
        Resolution,
        Runtime
    }

    sealed record NegativeTestMetadata
    {
        public NegativeTestPhase Phase { get; set; }
        public required string Type { get; set; }
    }

    public TestResultType ExpectedTestResultType => _metadata.Negative?.Phase switch
    {
        NegativeTestPhase.Parse => TestResultType.PARSING_FAILURE,
        NegativeTestPhase.Resolution => throw new MetadataParsingFailureException("Found negative module resolution test case, but there is no support for modules yet."),
        NegativeTestPhase.Runtime => TestResultType.FAILURE,
        null => TestResultType.SUCCESS,
        _ => throw new MetadataParsingFailureException($"Found invalid negative test case phase of {_metadata.Negative!.Phase}."),
    };

    public bool IsExpectedNegativeErrorType(string actualType) => actualType == _metadata.Negative!.Type;

    public List<string> Includes => _metadata.Includes ?? [];

    public bool HasFlag(string flag) => _metadata.Flags?.Contains(flag) ?? false;

    private readonly Metadata _metadata;
}
