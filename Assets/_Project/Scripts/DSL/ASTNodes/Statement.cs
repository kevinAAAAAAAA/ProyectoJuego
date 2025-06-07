
using System.Collections.Generic; 
using Assets._Project.Scripts.DSL;

public class RepeatStatement : Statement
{
    public Expression Times { get; }   
    public Statement Body { get; }     

    public RepeatStatement(Expression times, Statement body)
    {
        Times = times;
        Body = body;
    }
}