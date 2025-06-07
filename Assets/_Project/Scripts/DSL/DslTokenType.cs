using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;

namespace Assets._Project.Scripts.DSL
{



    public enum DslTokenType
    {
        KEYWORD_VAR,   
        IDENTIFIER,     
        EQUALS,       
        NUMBER,  
        STRING,        

        GAME_COMMAND,  

        LPAREN,      
        RPAREN,      
        COMMA,         

        NEWLINE,       
        EOF,           
        UNKNOWN,    
        ERROR,


        KEYWORD_SI,      
        KEYWORD_ENTONCES,   
        KEYWORD_SINO,      
        KEYWORD_FINSI,    

        LITERAL_TRUE,      
        LITERAL_FALSE,    


        KEYWORD_REPETIR,  
        KEYWORD_VECES,     
        KEYWORD_FINREPETIR, 








    }


}