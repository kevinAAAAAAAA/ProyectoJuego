using System.Collections.Generic;
using UnityEngine; 

using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
using System.Text;



namespace Assets._Project.Scripts.DSL
{




public class DslInterpreter
{
    private readonly PlayerController _playerController;
    private readonly GameManager _gameManager;
    private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();
    public List<string> RuntimeErrors { get; } = new List<string>();




    private StringGameEvent _dslErrorEvent;



    public DslInterpreter(PlayerController playerController, GameManager gameManager, StringGameEvent dslErrorEvent)
    {
        _playerController = playerController;
        _gameManager = gameManager;
        _dslErrorEvent = dslErrorEvent;
    }
    










    public void ProcessDSLExecution(string dslCode)
    {  
        Debug.Log($"====== DSLInterpreter: ProcessDSLExecution() RECIBIDO con código: '{dslCode}' ======");
        if (string.IsNullOrEmpty(dslCode))
        {
            if (_dslErrorEvent != null)
                _dslErrorEvent.Raise("No hay código para ejecutar.");
            else
                Debug.LogError("DSLErrorEvent no configurado en DslInterpreter.");
            return;
        }







        RuntimeErrors.Clear();
        _variables.Clear();
        
        if (_playerController != null) 
        {
            _playerController.ClearActionQueue(); 
        }
        else
        {
            Debug.LogError("[DSLInterpreter] PlayerController es null en ProcessDSLExecution. No se puede limpiar la cola de acciones.");
        }

        Debug.Log($"[DSLInterpreter] Procesando código: \n{dslCode}");




        DslLexer lexer = new DslLexer(dslCode);
        List<DslToken> tokens = lexer.ScanTokens();

        DslParser parser = new DslParser(tokens);
        List<Statement> statements = parser.Parse();

        if (parser.Errors.Count > 0)
        {
            StringBuilder sb = new StringBuilder("Errores de Sintaxis:\n");
            foreach (var error in parser.Errors)
            {
                sb.AppendLine(error);
            }
            if (_dslErrorEvent != null)
                _dslErrorEvent.Raise(sb.ToString());
            else
                Debug.LogError("DSLErrorEvent no configurado en DslInterpreter.");
            return;
        }

        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }

            Debug.Log("[DSLInterpreter] Interpretación completada sin errores de ejecución.");
        }
        catch (RuntimeError error)
        {
            RuntimeErrors.Add(error.Message); 
            if (_dslErrorEvent != null)
                _dslErrorEvent.Raise($"Error en Ejecución: {error.Message}");
            else
                Debug.LogError("DSLErrorEvent no configurado en DslInterpreter.");
        }
        finally
        {
        }
    }











































    public void Interpret(List<Statement> statements)
        {
            RuntimeErrors.Clear();
            _variables.Clear(); 

            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                RuntimeErrors.Add(error.Message);
                Debug.LogError(error.Message);
            }
        }


    private void Execute(Statement stmt)
    {
        if (stmt is VariableDeclarationStatement varDeclStmt)
        {
            object value = Evaluate(varDeclStmt.Initializer);
            Debug.Log($"[Interpreter] Declaring var '{varDeclStmt.Name.Lexeme}' with value: {value} (Type: {value?.GetType()})"); 
            _variables[varDeclStmt.Name.Lexeme] = value;
        }
        else if (stmt is AssignmentStatement assignStmt)
        {
             if (!_variables.ContainsKey(assignStmt.Name.Lexeme))
            {
                throw new RuntimeError(assignStmt.Name, $"Variable '{assignStmt.Name.Lexeme}' no ha sido declarada.");
            }
            object value = Evaluate(assignStmt.Value);
            _variables[assignStmt.Name.Lexeme] = value;
        }
        else if (stmt is GameCommandStatement cmdStmt)
        {
            ExecuteGameCommand(cmdStmt);
        }
        else if (stmt is IfStatement ifStmt)
        {
            ExecuteIfStatement(ifStmt);
        }
        else if (stmt is RepeatStatement repeatStmt) 
        {
            ExecuteRepeatStatement(repeatStmt);
        }
        else if (stmt is BlockStatement blockStmt)
        {
            ExecuteBlock(blockStmt);
        }
    }



    private void ExecuteRepeatStatement(RepeatStatement stmt)
    {
        object timesValue = Evaluate(stmt.Times);

        if (!(timesValue is int))
        {
            throw new RuntimeError(null, $"La expresión para REPETIR VECES debe evaluarse a un número entero. Se obtuvo: {timesValue} (tipo {timesValue?.GetType()})");

        }

        int repetitions = (int)timesValue;

        if (repetitions < 0)
        {
            Debug.LogWarning($"REPETIR VECES recibió un número negativo ({repetitions}). Se ejecutarán 0 veces.");
            repetitions = 0;
        }

        const int MAX_REPETITIONS = 1000; 
        if (repetitions > MAX_REPETITIONS)
        {
            throw new RuntimeError(null, $"El número de repeticiones ({repetitions}) excede el máximo permitido ({MAX_REPETITIONS}).");
        }

        for (int i = 0; i < repetitions; i++)
        {
            Execute(stmt.Body);

        }
    }




    public class RuntimeError : System.Exception
    {
        public DslToken Token { get; } 
        public RuntimeError(DslToken token, string message) : base(FormatMessage(token, message))
        {
            Token = token;
        }
        private static string FormatMessage(DslToken token, string message)
        {
            if (token != null) return $"Error en línea {token.Line} cerca de '{token.Lexeme}': {message}";
            return $"Error en tiempo de ejecución: {message}";
        }
    }

    
    private void ExecuteBlock(BlockStatement block)
    {
        foreach (var statement in block.Statements)
        {
            Execute(statement);
        }
    }

    private void ExecuteIfStatement(IfStatement stmt)
    {
        object conditionResult = Evaluate(stmt.Condition);
        Debug.Log($"[Interpreter] IF Condition evaluated to: {conditionResult} (Type: {conditionResult?.GetType()})");

        if (!IsTruthy(conditionResult))
        {
            Debug.Log("[Interpreter] IF Condition is FALSE. Checking ElseBranch.");
            if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
        }
        else
        {
            Debug.Log("[Interpreter] IF Condition is TRUE. Executing ThenBranch."); 
            Execute(stmt.ThenBranch);
        }
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null)
        {
            Debug.Log("[Interpreter] IsTruthy: Object is null, returning false.");
            return false;
        }
        if (obj is bool b)
        {
            Debug.Log($"[Interpreter] IsTruthy: Object is bool '{b}', returning {b}."); 
            return b;
        }
        Debug.LogError($"[Interpreter] IsTruthy: Condition must evaluate to VERDADERO o FALSO. Se obtuvo: {obj} (tipo {obj.GetType()})"); 
        throw new RuntimeError(null, $"La condición debe evaluarse a VERDADERO o FALSO. Se obtuvo: {obj} (tipo {obj.GetType()})");
    }


    private object Evaluate(Expression expr)
    {
        if (expr is LiteralExpression litExpr)
        {
            return litExpr.Value;
        }
        if (expr is VariableExpression varExpr)
        {
            if (_variables.TryGetValue(varExpr.Name.Lexeme, out object value))
            {
                Debug.Log($"[Interpreter] Evaluating var '{varExpr.Name.Lexeme}', found value: {value} (Type: {value?.GetType()})"); // LOG
                return value;
            }
            throw new RuntimeError(varExpr.Name, $"Variable indefinida '{varExpr.Name.Lexeme}'.");
        }
        if (expr is GameFunctionCallExpression funcCallExpr) //
        {
            return EvaluateGameFunctionCall(funcCallExpr);
        }
        throw new RuntimeError(null, "Expresión desconocida.");
    }

    private object EvaluateGameFunctionCall(GameFunctionCallExpression funcCallExpr)
    {
        string functionName = funcCallExpr.FunctionName.Lexeme.ToUpper();
        switch (functionName)
        {
            case "HAY_PARED_ADELANTE":
                return _playerController.CheckForWallAhead();
                case "TIENE_LLAVE":
            default:
                throw new RuntimeError(funcCallExpr.FunctionName, $"Función de condición desconocida: '{funcCallExpr.FunctionName.Lexeme}'.");
        }
    }



    private void ExecuteGameCommand(GameCommandStatement cmdStmt)
    {
        string commandName = cmdStmt.CommandName.Lexeme.ToUpper();
        List<object> arguments = new List<object>();
        foreach(var argExpr in cmdStmt.Arguments)
        {
            arguments.Add(Evaluate(argExpr));
        }

        switch (commandName)
        {
            case "MOVER_ADELANTE":
                if (arguments.Count == 1 && (arguments[0] is int || arguments[0] is float))
                {
                    int steps = (arguments[0] is float f) ? Mathf.RoundToInt(f) : (int)arguments[0];
                    _playerController.QueueMove(steps);
                }
                else
                {
                    throw new RuntimeError(cmdStmt.CommandName, "MOVER_ADELANTE espera 1 argumento numérico (pasos).");
                }
                break;
            case "GIRAR_IZQUIERDA":
                 if (arguments.Count == 0) _playerController.QueueTurn(-90);
                 else throw new RuntimeError(cmdStmt.CommandName, "GIRAR_IZQUIERDA no espera argumentos.");
                break;
            case "GIRAR_DERECHA":
                if (arguments.Count == 0) _playerController.QueueTurn(90);
                else throw new RuntimeError(cmdStmt.CommandName, "GIRAR_DERECHA no espera argumentos.");
                break;
            case "MOSTRAR":
                if (arguments.Count == 1)
                {
                    string messageToShow = arguments[0]?.ToString() ?? "NULO";
                    _gameManager.ShowMessageInUI("MOSTRAR: " + messageToShow); 
                }
                else
                {
                    throw new RuntimeError(cmdStmt.CommandName, "MOSTRAR espera 1 argumento.");
                }
                break;
                case "RECOGER_LLAVE":
                    if (cmdStmt.Arguments.Count == 0) _playerController.TryPickUpKey();
                    else throw new RuntimeError(cmdStmt.CommandName, "RECOGER_LLAVE no espera argumentos.");
                    break;
                case "ABRIR_PUERTA":
                    if (cmdStmt.Arguments.Count == 0) _playerController.TryOpenDoor();
                    else throw new RuntimeError(cmdStmt.CommandName, "ABRIR_PUERTA no espera argumentos.");
                    break;
            default:
                throw new RuntimeError(cmdStmt.CommandName, $"Comando de juego desconocido: '{cmdStmt.CommandName.Lexeme}'.");
        }
    }



}


}