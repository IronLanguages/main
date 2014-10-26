using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    internal class PythonByteCode {
        public const int CO_MAXBLOCKS = 20; // same as in CPython
        private const int CALL_FLAG_VAR = 1;
        private const int CALL_FLAG_KW = 2;

        enum Opcode {
            POP_TOP = 1,
            ROT_TWO = 2,
            ROT_THREE = 3,
            DUP_TOP = 4,
            ROT_FOUR = 5,
            NOP = 9,
            UNARY_POSITIVE = 10,
            UNARY_NEGATIVE = 11,
            UNARY_NOT = 12,
            UNARY_CONVERT = 13,
            UNARY_INVERT = 15,
            LIST_APPEND = 18,
            BINARY_POWER = 19,
            BINARY_MULTIPLY = 20,
            BINARY_DIVIDE = 21,
            BINARY_MODULO = 22,
            BINARY_ADD = 23,
            BINARY_SUBTRACT = 24,
            BINARY_SUBSCR = 25,
            BINARY_FLOOR_DIVIDE = 26,
            BINARY_TRUE_DIVIDE = 27,
            INPLACE_FLOOR_DIVIDE = 28,
            INPLACE_TRUE_DIVIDE = 29,
            SLICE = 30,
            /* Also uses 31-33 */
            STORE_SLICE = 40,
            /* Also uses 41-43 */
            DELETE_SLICE = 50,
            /* Also uses 51-53 */
            INPLACE_ADD = 55,
            INPLACE_SUBTRACT = 56,
            INPLACE_MULTIPLY = 57,
            INPLACE_DIVIDE = 58,
            INPLACE_MODULO = 59,
            STORE_SUBSCR = 60,
            DELETE_SUBSCR = 61,
            BINARY_LSHIFT = 62,
            BINARY_RSHIFT = 63,
            BINARY_AND = 64,
            BINARY_XOR = 65,
            BINARY_OR = 66,
            INPLACE_POWER = 67,
            GET_ITER = 68,
            PRINT_EXPR = 70,
            PRINT_ITEM = 71,
            PRINT_NEWLINE = 72,
            PRINT_ITEM_TO = 73,
            PRINT_NEWLINE_TO = 74,
            INPLACE_LSHIFT = 75,
            INPLACE_RSHIFT = 76,
            INPLACE_AND = 77,
            INPLACE_XOR = 78,
            INPLACE_OR = 79,
            BREAK_LOOP = 80,
            WITH_CLEANUP = 81,
            LOAD_LOCALS = 82,
            RETURN_VALUE = 83,
            IMPORT_STAR = 84,
            EXEC_STMT = 85,
            YIELD_VALUE = 86,
            POP_BLOCK = 87,
            END_FINALLY = 88,
            BUILD_CLASS = 89,
            HAVE_ARGUMENT = 90,	/* Opcodes from here have an argument: */

            STORE_NAME = 90,	/* Index in name list */

            DELETE_NAME = 91,	/* "" */

            UNPACK_SEQUENCE = 92,	/* Number of sequence items */

            FOR_ITER = 93,
            STORE_ATTR = 95,	/* Index in name list */

            DELETE_ATTR = 96,	/* "" */

            STORE_GLOBAL = 97,	/* "" */

            DELETE_GLOBAL = 98,	/* "" */

            DUP_TOPX = 99,	/* number of items to duplicate */

            LOAD_CONST = 100,	/* Index in const list */

            LOAD_NAME = 101,	/* Index in name list */

            BUILD_TUPLE = 102,	/* Number of tuple items */

            BUILD_LIST = 103,	/* Number of list items */

            BUILD_MAP = 104,	/* Always zero for now */

            LOAD_ATTR = 105,	/* Index in name list */

            COMPARE_OP = 106,	/* Comparison operator */

            IMPORT_NAME = 107,	/* Index in name list */

            IMPORT_FROM = 108,	/* Index in name list */

            JUMP_FORWARD = 110,	/* Number of bytes to skip */

            JUMP_IF_FALSE = 111,	/* "" */

            JUMP_IF_TRUE = 112,	/* "" */

            JUMP_ABSOLUTE = 113,	/* Target byte offset from beginning of code */

            LOAD_GLOBAL = 116,	/* Index in name list */

            CONTINUE_LOOP = 119,	/* Start of loop (absolute) */

            SETUP_LOOP = 120,	/* Target address (absolute) */

            SETUP_EXCEPT = 121,	/* "" */

            SETUP_FINALLY = 122,	/* "" */

            LOAD_FAST = 124,	/* Local variable number */

            STORE_FAST = 125,	/* Local variable number */

            DELETE_FAST = 126,	/* Local variable number */

            RAISE_VARARGS = 130,	/* Number of raise arguments (1, 2 or 3) */
            /* CALL_FUNCTION_XXX opcodes defined below depend on this definition */

            CALL_FUNCTION = 131,	/* #args + (#kwargs<<8) */

            MAKE_FUNCTION = 132,	/* #defaults */

            BUILD_SLICE = 133,	/* Number of items */

            MAKE_CLOSURE = 134,     /* #free vars */

            LOAD_CLOSURE = 135,     /* Load free variable from closure */

            LOAD_DEREF = 136,     /* Load and dereference from closure cell */

            STORE_DEREF = 137,     /* Store into cell */

            /* The next 3 opcodes must be contiguous and satisfy
            (CALL_FUNCTION_VAR - CALL_FUNCTION) & 3 == 1  */
            CALL_FUNCTION_VAR = 140,	/* #args + (#kwargs<<8) */

            CALL_FUNCTION_KW = 141,	/* #args + (#kwargs<<8) */

            CALL_FUNCTION_VAR_KW = 142,	/* #args + (#kwargs<<8) */

            /* Support for opargs more than 16 bits long */
            EXTENDED_ARG = 143,
        }

        public PythonByteCode (int argcount, int nlocals, int stacksize, int flags,
            string code, PythonTuple constants, PythonTuple names, PythonTuple varnames,
            string filename, string name, int firstlineno, string lnotab,
            PythonTuple cellvars = null, PythonTuple freevars = null) {

            co_code = PythonAsciiEncoding.ASCII.GetBytes (code);
            co_consts = constants;
        }

        public object Interpret (ModuleContext context) {
            int opcode;
            int next_instr = 0;
            Stack<object> stack = new Stack<object> ();
            int oparg = 0;
            object retval = null;
            while (true) {
                opcode = co_code[next_instr];
                if (opcode >= (int)Opcode.HAVE_ARGUMENT) {
                    next_instr += 2;
                    oparg = ((int)co_code[next_instr]) << 8 + (int)(co_code[next_instr - 1]);
                }
                next_instr += 1;

                switch ((Opcode)opcode) {
                    case Opcode.NOP:
                        break;

                    case Opcode.LOAD_FAST:
                        break;

                    case Opcode.LOAD_CONST:
                        stack.Push (co_consts[oparg]);
                        break;

                    case Opcode.STORE_FAST:
                        break;

                    case Opcode.POP_TOP:
                        stack.Pop ();
                        break;

                    case Opcode.ROT_TWO:
                        break;

                    case Opcode.ROT_THREE:
                        break;

                    case Opcode.ROT_FOUR:
                        break;

                    case Opcode.DUP_TOP:
                        break;

                    case Opcode.DUP_TOPX:
                        if (oparg == 2 || oparg == 3) {

                        } else {
                            throw PythonOps.RuntimeError ("invalid argument to DUP_TOPX (bytecode corruption?)");
                        }
                        break;

                    case Opcode.UNARY_POSITIVE:

                        break;

                    case Opcode.UNARY_NEGATIVE:
                        break;

                    case Opcode.UNARY_NOT:
                        break;

                    case Opcode.UNARY_CONVERT:
                        break;

                    case Opcode.UNARY_INVERT:
                        break;

                    case Opcode.BINARY_POWER:
                        break;

                    case Opcode.BINARY_MULTIPLY:
                        break;

                    case Opcode.BINARY_DIVIDE:
                        break;

                    case Opcode.BINARY_TRUE_DIVIDE:
                        break;

                    case Opcode.BINARY_FLOOR_DIVIDE:
                        break;

                    case Opcode.BINARY_MODULO:
                        break;

                    case Opcode.BINARY_ADD:
                        break;

                    case Opcode.BINARY_SUBTRACT:
                        break;

                    case Opcode.BINARY_SUBSCR:
                        break;

                    case Opcode.BINARY_LSHIFT:
                        break;

                    case Opcode.BINARY_RSHIFT:
                        break;

                    case Opcode.BINARY_AND:
                        break;

                    case Opcode.BINARY_XOR:
                        break;

                    case Opcode.BINARY_OR:{
                        //object b = stack.Pop();
                        //object a = stack.Pop();
                        //stack.Push(a._or(b));
                        break;
                    }

                    case Opcode.LIST_APPEND: {
                        object b = stack.Pop ();
                        List a = stack.Pop () as List;
                        a.append (b);
                        break;
                    }

                    case Opcode.INPLACE_POWER:
                        break;

                    case Opcode.INPLACE_MULTIPLY:
                        break;

                    case Opcode.INPLACE_DIVIDE:
                        break;

                    case Opcode.INPLACE_TRUE_DIVIDE:
                        break;

                    case Opcode.INPLACE_FLOOR_DIVIDE:
                        break;

                    case Opcode.INPLACE_MODULO:
                        break;

                    case Opcode.INPLACE_ADD:
                        break;

                    case Opcode.INPLACE_SUBTRACT:
                        break;

                    case Opcode.INPLACE_LSHIFT:
                        break;

                    case Opcode.INPLACE_RSHIFT:
                        break;

                    case Opcode.INPLACE_AND:
                        break;

                    case Opcode.INPLACE_XOR:
                        break;

                    case Opcode.INPLACE_OR:
                        break;

                    case Opcode.SLICE + 0:
                    case Opcode.SLICE + 1:
                    case Opcode.SLICE + 2:
                    case Opcode.SLICE + 3: {
                        //object stop = (((opcode - (int)Opcode.SLICE) & 2) != 0) ? stack.Pop () : null;
                        //object start = (((opcode - (int)Opcode.SLICE) & 1) != 0) ? stack.Pop () : null;
                        //object obj = stack.Pop ();
                        //stack.Push (obj.__getslice__ (start, stop));
                        break;
                    }

                    case Opcode.STORE_SLICE + 0:
                    case Opcode.STORE_SLICE + 1:
                    case Opcode.STORE_SLICE + 2:
                    case Opcode.STORE_SLICE + 3:
                        break;

                    case Opcode.DELETE_SLICE + 0:
                    case Opcode.DELETE_SLICE + 1:
                    case Opcode.DELETE_SLICE + 2:
                    case Opcode.DELETE_SLICE + 3:
                        break;

                    case Opcode.STORE_SUBSCR: {
                            object key = stack.Pop ();
                            object obj = stack.Pop ();
                            object value = stack.Pop ();
                            PythonOps.Invoke (context.GlobalContext, obj, "__setitem__", key, value);
                            break;
                        }

                    case Opcode.DELETE_SUBSCR: {
                            object key = stack.Pop ();
                            object obj = stack.Pop ();
                            PythonOps.Invoke (context.GlobalContext, obj, "__delitem__", key);
                            break;
                        }

                    case Opcode.PRINT_EXPR:
                        break;

                    case Opcode.PRINT_ITEM_TO:
                        PythonOps.PrintCommaWithDest (context.GlobalContext, stack.Pop (), stack.Pop ());
                        break;

                    case Opcode.PRINT_ITEM:
                        PythonOps.Print (context.GlobalContext, stack.Pop ());
                        break;

                    case Opcode.PRINT_NEWLINE_TO:
                        PythonOps.PrintNewlineWithDest (context.GlobalContext, stack.Pop ());
                        break;

                    case Opcode.PRINT_NEWLINE:
                        PythonOps.PrintNewline (context.GlobalContext);
                        break;

                    case Opcode.RAISE_VARARGS:
                        break;

                    case Opcode.LOAD_LOCALS:

                        break;

                    case Opcode.RETURN_VALUE:
                        retval = stack.Pop ();
                        break;

                    case Opcode.YIELD_VALUE:
                        break;

                    case Opcode.EXEC_STMT:
                        break;

                    case Opcode.POP_BLOCK:
                        break;

                    case Opcode.END_FINALLY:
                        break;

                    case Opcode.BUILD_CLASS:
                        break;

                    case Opcode.STORE_NAME:
                        PythonOps.SetLocal (context.GlobalContext, co_names[oparg], stack.Pop ());
                        break;

                    case Opcode.DELETE_NAME:
                        PythonOps.DeleteLocal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.UNPACK_SEQUENCE:
                        break;

                    case Opcode.STORE_ATTR: {
                            object obj = stack.Pop ();
                            object v = stack.Pop ();
                            PythonOps.SetAttr (context.GlobalContext, obj, co_names[oparg], v);
                            break;
                        }

                    case Opcode.DELETE_ATTR:
                        PythonOps.DeleteAttr (context.GlobalContext, stack.Pop (), co_names[oparg]);
                        break;

                    case Opcode.STORE_GLOBAL:
                        PythonOps.SetGlobal (context.GlobalContext, co_names[oparg], stack.Pop ());
                        break;

                    case Opcode.DELETE_GLOBAL:
                        PythonOps.DeleteGlobal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.LOAD_NAME:
                        PythonOps.GetLocal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.LOAD_GLOBAL:
                        stack.Push (PythonOps.GetGlobal (context.GlobalContext, co_names[oparg]));
                        break;

                    case Opcode.DELETE_FAST:
                        break;

                    case Opcode.LOAD_CLOSURE:
                        break;

                    case Opcode.LOAD_DEREF:
                        break;

                    case Opcode.STORE_DEREF:
                        break;

                    case Opcode.BUILD_TUPLE:
                        //                        PythonTuple res = PythonOps.MakeTuple()
                        for (int i = 0; i < oparg; i++) {
                            //res.

                        }
                        break;

                    case Opcode.BUILD_LIST:
                        List l = new List ();
                        for (int i = 0; i < oparg; i++) {
                            l.Add (stack.Pop ());
                        }
                        stack.Push (l);
                        break;

                    case Opcode.BUILD_MAP:
                        stack.Push (new PythonDictionary ());
                        break;

                    case Opcode.LOAD_ATTR:
                        stack.Push (PythonOps.ObjectGetAttribute (context.GlobalContext, stack.Pop (), co_names[oparg]));
                        break;

                    case Opcode.COMPARE_OP:
                        break;

                    case Opcode.IMPORT_NAME:

                        break;

                    case Opcode.IMPORT_STAR:

                        break;

                    case Opcode.IMPORT_FROM:
                        break;

                    case Opcode.JUMP_FORWARD:
                        next_instr += oparg;
                        break;

                    case Opcode.JUMP_IF_FALSE:
                        if (!PythonOps.CheckingConvertToNonZero (stack.Peek ())) {
                            next_instr += oparg;
                        }
                        break;

                    case Opcode.JUMP_IF_TRUE:
                        if (PythonOps.CheckingConvertToNonZero (stack.Peek ())) {
                            next_instr += oparg;
                        }
                        break;

                    case Opcode.JUMP_ABSOLUTE:
                        next_instr = oparg;
                        break;

                    case Opcode.GET_ITER:
                        //                        object it = PythonOps.Istack.Peek().__iter__();
                        //1002                        if (it != null) {
                        //1003                            stack.set_top(it);
                        //1004                        }
                        break;

                    case Opcode.FOR_ITER:
                        break;

                    case Opcode.BREAK_LOOP:
                        break;

                    case Opcode.CONTINUE_LOOP:
                        break;

                    case Opcode.SETUP_LOOP:
                    case Opcode.SETUP_EXCEPT:
                    case Opcode.SETUP_FINALLY:
                        break;

                    case Opcode.WITH_CLEANUP:
                        /* TOP is the context.__exit__ bound method.
                        Below that are 1-3 values indicating how/why
                        we entered the finally clause:
                        - SECOND = None
                        - (SECOND, THIRD) = (WHY_{RETURN,CONTINUE}), retval
                        - SECOND = WHY_*; no retval below it
                        - (SECOND, THIRD, FOURTH) = exc_info()
                        In the last case, we must call
                        TOP(SECOND, THIRD, FOURTH)
                        otherwise we must call
                        TOP(None, None, None)

                        In addition, if the stack represents an exception,
                         *and* the function call returns a 'true' value, we
                        "zap" this information, to prevent END_FINALLY from
                        re-raising the exception.  (But non-local gotos
                        should still be resumed.)
                         */
                        break;

                    case Opcode.CALL_FUNCTION:
                        break;

                    case Opcode.CALL_FUNCTION_VAR:
                    case Opcode.CALL_FUNCTION_KW:
                    case Opcode.CALL_FUNCTION_VAR_KW:
                        break;

                    case Opcode.MAKE_FUNCTION:
                        break;

                    case Opcode.MAKE_CLOSURE:
                        break;

                    case Opcode.BUILD_SLICE:
                        object step = (oparg == 3) ? stack.Pop () : null;
                        object stop = stack.Pop ();
                        object start = stack.Pop ();
                        stack.Push (PythonOps.MakeSlice (start, stop, step));
                        break;

                    case Opcode.EXTENDED_ARG:
                        opcode = co_code[next_instr++];
                        next_instr += 2;
                        oparg = oparg << 16 | ((co_code[next_instr] << 8) + co_code[next_instr - 1]);
                        break;

                    default:
                        break;
                }

                if (next_instr >= co_code.Length)
                    break;
            }

            return retval;
        }

        public byte[] co_code {
            get;
            private set;
        }

        public PythonTuple co_consts {
            get;
            private set;
        }

        public string[] co_names {
            get;
            private set;
        }

        //public int co
    }
}
