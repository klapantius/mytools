using System;
using System.Text.RegularExpressions;

using Microsoft.TeamFoundation.Build.Client;


namespace testrunanalyzer
{
    public class BuildSpecInjector
    {
        public void Inject(string buildNameSpecification, ref IBuildDetailSpec targetBuildDetailSpec)
        {
            if (new Regex(@"\d$").IsMatch(buildNameSpecification))
            {
                targetBuildDetailSpec.BuildNumber = buildNameSpecification;
            }
            else
            {
                targetBuildDetailSpec.DefinitionSpec.Name = buildNameSpecification;
            }

        }
    }
}
