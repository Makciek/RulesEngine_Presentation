// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RulesEngineBenchmark
{
    using System.Linq;
    using System.Text.Json;

    [MemoryDiagnoser]
    public class REBenchmark
    {
        private readonly RulesEngine.RulesEngine rulesEngine;
        private readonly object ruleInput_Dynamic;
        private readonly Input ruleInput;
        private readonly List<Workflow> workflow;

        public REBenchmark()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "NestedInputDemo.json",
                SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                throw new Exception("Rules not found.");
            }

            var fileData = File.ReadAllText(files[0]);
            workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData);

            rulesEngine = new RulesEngine.RulesEngine(workflow.ToArray(), new ReSettings
            {
                EnableFormattedErrorMessage = false,
                EnableScopedParams = false
            });

            this.ruleInput_Dynamic = new
            {
                SimpleProp = "simpleProp",
                NestedProp = new
                {
                    SimpleProp = "nestedSimpleProp",
                    ListProp = new List<ListItem>
                    {
                        new ListItem
                        {
                            Id = 1,
                            Value = "first"
                        },
                        new ListItem
                        {
                            Id = 2,
                            Value = "second"
                        }
                    }
                }
            };

            this.ruleInput = new Input()
            {
                
                SimpleProp = "simpleProp",
                NestedProp = new NestedInput()
                {
                    SimpleProp = "nestedSimpleProp",
                    ListProp = new List<ListItem>
                    {
                        new ListItem
                        {
                            Id = 1,
                            Value = "first"
                        },
                        new ListItem
                        {
                            Id = 2,
                            Value = "second"
                        }
                    }
                }
            };
        }

        [Params(1000, 10000)] public int N;

        [Benchmark]
        public void RuleExecutionDefault_Dynamic()
        {
            foreach (var workflow in workflow)
            {
                _ = rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, this.ruleInput_Dynamic).Result;
            }
        }

        [Benchmark]
        public void RuleExecutionDefault()
        {
            foreach (var workflow in workflow)
            {
                _ = rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, this.ruleInput).Result;
            }
        }

        [Benchmark]
        public void PlainCSharp_Dynamic()
        {
            foreach (var workflow in workflow)
            {
                var results = new List<(bool, string)>();
                results.Add(Workflow1_Dynamic(this.ruleInput_Dynamic));
                results.Add(Workflow2_Dynamic(this.ruleInput_Dynamic));
                results.Add(Workflow3_Dynamic(this.ruleInput_Dynamic));
                counter += results.Count(r => r.Item1);
            }
        }

        [Benchmark]
        public void PlainCSharp()
        {
            foreach (var workflow in workflow)
            {
                var results = new List<(bool, string)>();
                results.Add(Workflow1(this.ruleInput));
                results.Add(Workflow2(this.ruleInput));
                results.Add(Workflow3(this.ruleInput));
                counter += results.Count(r => r.Item1);
            }
        }

        private long counter = 0;

        private static (bool, string) Workflow1_Dynamic(dynamic ruleInput)
        {
            var result = ruleInput.NestedProp.SimpleProp == "nestedSimpleProp";
            return (result, result ? null : "One or more adjust rules failed.");
        }

        private static (bool, string) Workflow2_Dynamic(dynamic ruleInput)
        {
            var result = ruleInput.NestedProp.ListProp[0].Id == 1 && ruleInput.NestedProp.ListProp[1].Value == "second";
            return (result, result ? null : "One or more adjust rules failed.");
        }

        private static (bool, string) Workflow3_Dynamic(dynamic ruleInput)
        {
            var result = ruleInput.NestedProp.ListProp[1].Value.ToUpper() == "SECOND";
            return (result, result ? null : "One or more adjust rules failed.");
        }

        private static (bool, string) Workflow1(Input ruleInput)
        {
            var result = ruleInput.NestedProp.SimpleProp == "nestedSimpleProp";
            return (result, result ? null : "One or more adjust rules failed.");
        }

        private static (bool, string) Workflow2(Input ruleInput)
        {
            var result = ruleInput.NestedProp.ListProp[0].Id == 1 && ruleInput.NestedProp.ListProp[1].Value == "second";
            return (result, result ? null : "One or more adjust rules failed.");
        }

        private static (bool, string) Workflow3(Input ruleInput)
        {
            var result = ruleInput.NestedProp.ListProp[1].Value.ToUpper() == "SECOND";
            return (result, result ? null : "One or more adjust rules failed.");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<REBenchmark>();
        }
    }

    public class Input
    {
        public string SimpleProp { get; set; }

        public NestedInput NestedProp { get; set; }
    }

    public class NestedInput
    {
        public string SimpleProp { get; set; }
        public List<ListItem> ListProp { get; set; }
    }

    public class ListItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
