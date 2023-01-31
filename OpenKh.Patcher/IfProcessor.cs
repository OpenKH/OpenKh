using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpenKh.Patcher
{
    public class IfProcessor
    {
        private static readonly INamingConvention _namingConvention = YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance;

        private readonly TemplateContext _context;

        public IfProcessor(Action<Action<string, object>> registration)
        {
            var script = new Scriban.Runtime.ScriptObject();

            registration?.Invoke(
                (key, value) =>
                {
                    script[key] = value;
                }
            );

            _context = new TemplateContext(script)
            {
                MemberRenamer = memberInfo => _namingConvention.Apply(memberInfo.Name),
            };
        }

        public bool EvalIf(string expression)
        {
            var result = Template.Evaluate(expression, _context);

            return false
                || (result is bool boolValue && boolValue)
                || (result is string stringValue && stringValue == "1")
                ;
        }
    }
}
