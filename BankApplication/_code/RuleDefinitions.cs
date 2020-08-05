using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp._code
{
    public static class RuleGenerator
    {
        public class MathKeywordRule : IKeywordRule
        {
            public string Rule { get; private set; }
            public Category TargetCategory { get; private set; }

            private bool _lessThan;
            private int _amount;

            /// <summary>
            /// Something similar to "value < 250 && ' PG 205'"
            /// </summary>
            /// <param name="rule"></param>
            /// <param name="cat"></param>
            /// <param name="amount"></param>
            /// <param name="lessThan">If rule should trigger if amount is less than said value or higher than said value</param>
            public MathKeywordRule(string rule, Category cat, int amount, bool lessThan)
            {
                Rule = rule;
                TargetCategory = cat;
                _amount = amount;
                _lessThan = lessThan;
            }

            public bool IsRuleTriggeredByTransaction(ViewTransaction transaction)
            {
                bool amountInRange = _lessThan ? transaction.Amount < _amount : transaction.Amount >= _amount;
                if (amountInRange)
                {
                    return transaction.Description.Contains(Rule);
                }
                return false;
            }
        }

        public class KeywordRule : IKeywordRule
        {
            public string Rule { get; private set; }
            public Category TargetCategory { get; private set; }

            public KeywordRule(string rule, Category category)
            {
                Rule = rule;
                TargetCategory = category;
            }

            public bool IsRuleTriggeredByTransaction(ViewTransaction transaction)
            {
                return transaction.Description.Contains(Rule);
            }
        }

        public interface IKeywordRule
        {
            string Rule { get; }
            Category TargetCategory { get; }

            bool IsRuleTriggeredByTransaction(ViewTransaction transaction);
        }
    }
}
