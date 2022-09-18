using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Flee.PublicTypes;
using Hawk.ARINC717;
using Hawk.Decode.Configuration.Model;
using IDynamicExpression = Flee.PublicTypes.IDynamicExpression;

namespace Hawk.Decode
{
    public class FDRDecoder
    {
        private Dictionary<string, IDynamicExpression> _functionContexts = new Dictionary<string, IDynamicExpression>();
        private Dictionary<string, object> _lastParameterValues = new Dictionary<string, object>();

        private DataFrameConfiguration _configuration;
        public FDRDecoder(DataFrameConfiguration configuration)
        {
            _configuration = configuration;
            BuildFunctionContexts();
        }

        public Dictionary<string, List<object>> DecodeSubframe(byte[] subframeData)
        {
            Dictionary<string, List<object>> values = new Dictionary<string, List<object>>();
            Subframe subframe;
            switch (_configuration.ARINC717?.WordFormat)
            {
                case ARINC717WordFormat.LittleEndianUnpacked16BitWords:
                    
                    UInt16[] words = ConvertWordDataFromLittleEndian16Bits(subframeData);
                    subframe = new Subframe(words);
                    break;
                default:
                    throw new NotImplementedException("Other word formats are currently not supported");
            }

            if (subframe.SubFrameNumber == -1)
                throw new DecodeException("Invalid subframe found, missing sync word.");

            ProcessSubframe(subframe, values);

            return values;
        }

        private void ProcessSubframe(Subframe subframe, Dictionary<string, List<object>> values)
        {
            foreach (var parameter in _configuration.ARINC717!.Parameters)
            {
                foreach (var location in parameter.Locations)
                {
                    UInt16 rawWord = subframe.WordRawData(location.Word);
                    object value = ARINC717Decode.Decode(parameter, location, rawWord);

                    if (!values.ContainsKey(parameter.Name))
                        values.Add(parameter.Name, new List<object>());

                    values[parameter.Name].Add(value);
                }
            }

            if (_configuration.Functions != null)
            {
                foreach (var function in _configuration.Functions)
                {
                    object? functionValue = null;
                    switch (function.ReturnType)
                    {
                        case FunctionReturnType.Double:
                            functionValue = DecodeFunction<double?>(function);
                            break;
                        case FunctionReturnType.Boolean:
                            functionValue = DecodeFunction<bool?>(function);
                            break;
                        case FunctionReturnType.Integer:
                            functionValue = DecodeFunction<int?>(function);
                            break;
                        case FunctionReturnType.String:
                            functionValue = DecodeFunction<string>(function);
                            break;
                    }

                    if (functionValue != null)
                    {
                        if (!values.ContainsKey(function.Name))
                            values.Add(function.Name, new List<object>());

                        values[function.Name].Add(functionValue);

                        if (_lastParameterValues.ContainsKey(function.Name))
                        {
                            _lastParameterValues[function.Name] = functionValue;
                        }
                        else
                        {
                            _lastParameterValues.Add(function.Name, functionValue);
                        }
                    }
                }
            }
        }

        private bool IsParameterLocationInSubframeAndSuperframe(ARINC717ParameterLocation location, Subframe subframe, int superframe)
        {
            bool isInSubframe = location.Subframe == null || location.Subframe == subframe.SubFrameNumber;
            bool isInSuperframe = location.Superframe == null || location.Superframe == superframe;

            return isInSubframe && isInSuperframe;
        }

        private UInt16[] ConvertWordDataFromLittleEndian16Bits(byte[] subframeData)
        {
            if (subframeData.Length != _configuration.ARINC717!.WordsPerSecond * 2)
                throw new ArgumentException("Invalid subframe data, we were expecting a buffer twice the length of the wps");

            UInt16[] wordData = new UInt16[_configuration.ARINC717.WordsPerSecond];
            for (int i = 0; i < subframeData.Length; i += 2)
            {
                wordData[i / 2] = (UInt16)((subframeData[i + 1] << 8) | subframeData[i]);
            }

            return wordData;
        }

        private T? DecodeFunction<T>(Function f)
        {
            try
            {
                var dynamicExpresstion = _functionContexts[f.Name];
                return (T)dynamicExpresstion.Evaluate();
            }
            catch
            {
                return default(T);
            }
        }

        private void BuildFunctionContexts()
        {
            _functionContexts.Clear();

            if (_configuration.Functions == null)
                return;

            ExpressionContext context = new ExpressionContext();
            // Allow the expression to use all static public methods of System.Math
            context.Imports.AddType(typeof(Math));
            context.Imports.AddType(typeof(CustomFunctions));

            VariableCollection variables = context.Variables;
            variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(FunctionResolveVariableType!);
            variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(FunctionResolveVariableValue!);


            foreach (var function in _configuration.Functions)
            {

                var expression = function.Expr;
                foreach (Match variableMatch in Regex.Matches(function.Expr, "{(.*?)}"))
                {
                    //remove the spaces and replace with underscores
                    var variableText = variableMatch.Groups[1].Value.Replace(" ", "_").Replace("/", "_"); ;

                    //replace the old variable text which was in the format {variable name} with variable_name.
                    //this is needed for flee.
                    expression = expression.Replace(variableMatch.Value, variableText);
                }

                IDynamicExpression eDynamic = context.CompileDynamic(expression);
                _functionContexts[function.Name] = eDynamic;
            }
        }

        private void FunctionResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
        {
            e.VariableType = typeof(double); //ALL are double's, except for other functions.  But we dont support functions of functions as of writing. 
        }

        private void FunctionResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
        {
            if (_lastParameterValues.ContainsKey(e.VariableName))
            {
                e.VariableValue = _lastParameterValues[e.VariableName];
            }
            else
            {
                throw new Exception("No data for variable " + e.VariableName);
            }
        }
    }
}

