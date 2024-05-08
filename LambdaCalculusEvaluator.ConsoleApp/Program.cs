using LambdaCalculusEvaluator.Library;
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("(λa.λb.λc.λd.(((ab)c)d)(bc))".Evaluate());
//Result: λe.λf.λd.((((bc)e)f)d)
