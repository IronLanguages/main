using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    internal class PythonByteCode {
        public const int CO_MAXBLOCKS = 20; // same as in CPython (Include/code.h)
        private const int CALL_FLAG_VAR = 1;
        private const int CALL_FLAG_KW = 2;

        enum Opcode {
            STOP_CODE = 0,
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
            SLICE = 30, /* Also uses 31-33 */
            STORE_SLICE = 40, /* Also uses 41-43 */
            DELETE_SLICE = 50, /* Also uses 51-53 */
            STORE_MAP = 54,             
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

            LIST_APPEND	= 94,

            STORE_ATTR = 95,	/* Index in name list */

            DELETE_ATTR = 96,	/* "" */

            STORE_GLOBAL = 97,	/* "" */

            DELETE_GLOBAL = 98,	/* "" */

            DUP_TOPX = 99,	/* number of items to duplicate */

            LOAD_CONST = 100,	/* Index in const list */

            LOAD_NAME = 101,	/* Index in name list */

            BUILD_TUPLE = 102,	/* Number of tuple items */

            BUILD_LIST = 103,	/* Number of list items */

            BUILD_SET = 104, /* Number of set items */

            BUILD_MAP = 105,	/* Always zero for now */

            LOAD_ATTR = 106,	/* Index in name list */

            COMPARE_OP = 107,	/* Comparison operator */

            IMPORT_NAME = 108,	/* Index in name list */

            IMPORT_FROM = 109,	/* Index in name list */

            JUMP_FORWARD = 110,	/* Number of bytes to skip */

            JUMP_IF_FALSE_OR_POP = 111,	/* "" */

            JUMP_IF_TRUE_OR_POP = 112,	/* "" */

            JUMP_ABSOLUTE = 113,    /* Target byte offset from beginning of code */

            POP_JUMP_IF_FALSE = 114,	/* "" */
            POP_JUMP_IF_TRUE = 115,	/* "" */

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

            SETUP_WITH = 143, 
            
            /* Support for opargs more than 16 bits long */
            EXTENDED_ARG = 145,

            SET_ADD = 146,
            MAP_ADD = 147,
        }

        /* Rich comparison opcodes */
        enum cmp_op {
            PyCmp_LT = 0, // Py_LT
            PyCmp_LE = 1, // Py_LE
            PyCmp_EQ = 2, // Py_EQ
            PyCmp_NE = 3, // Py_NE
            PyCmp_GT = 4, // Py_GT
            PyCmp_GE = 5, // Py_GE
            PyCmp_IN,
            PyCmp_NOT_IN,
            PyCmp_IS,
            PyCmp_IS_NOT,
            PyCmp_EXC_MATCH,
            PyCmp_BAD
        }

        public PythonByteCode (int argcount, int nlocals, int stacksize, int flags,
            string code, PythonTuple constants, PythonTuple names, PythonTuple varnames,
            string filename, string name, int firstlineno, string lnotab,
            PythonTuple cellvars = null, PythonTuple freevars = null) {

            co_code = PythonAsciiEncoding.ASCII.GetBytes (code);
            co_consts = constants;
            co_varnames = varnames;
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
                    case Opcode.STOP_CODE: // Indicates end-of-code to the compiler, not used by the interpreter.
                        break;

                    case Opcode.NOP: // Do nothing code. Used as a placeholder by the bytecode optimizer
                        break;

                    case Opcode.LOAD_FAST:  // Pushes a reference to the local co_varnames[var_num] onto the stack.
                        throw new NotImplementedException("LOAD_FAST");
                        break;

                    case Opcode.LOAD_CONST: // Pushes co_consts[consti] onto the stack.
                        stack.Push (co_consts[oparg]);
                        break;

                    case Opcode.STORE_FAST: // Stores TOS into the local co_varnames[var_num]
                        throw new NotImplementedException("STORE_FAST");
                        break;

                    case Opcode.POP_TOP: // Removes the top-of-stack (TOS) item.
                        stack.Pop ();
                        break;

                    case Opcode.ROT_TWO: // Swaps the two top-most stack items.
                        {
                            object a = stack.Pop();
                            object b = stack.Pop();
                            stack.Push(a);
                            stack.Push(b);
                        } 
                        break;

                    case Opcode.ROT_THREE: // Lifts second and third stack item one position up, moves top down to position three.
                        {
                            object v = stack.Pop();
                            object w = stack.Pop();
                            object x = stack.Pop();
                            stack.Push(w);
                            stack.Push(x);
                            stack.Push(v);                            
                        }
                        break;

                    case Opcode.ROT_FOUR: // Lifts second, third and forth stack item one position up, moves top down to position four.
                        {
                            object u = stack.Pop();
                            object v = stack.Pop();
                            object w = stack.Pop();
                            object x = stack.Pop();
                            stack.Push(u);
                            stack.Push(x);
                            stack.Push(w);
                            stack.Push(v);
                        }
                        break;

                    case Opcode.DUP_TOP: // Duplicates the reference on top of the stack.
                        stack.Push(stack.Peek());
                        break;

                    case Opcode.DUP_TOPX/*(count)*/: // Duplicate count items, keeping them in the same order.Due to implementation limits, count should be between 1 and 5 inclusive.
                        if (oparg == 2 || oparg == 3) {
                            for(int i = 0; i < oparg; i++) {
                                stack.Push(stack.Peek());
                            }    
                        } else {
                            throw PythonOps.RuntimeError ("invalid argument to DUP_TOPX (bytecode corruption?)");
                        }
                        break;

                    // Unary Operations take the top of the stack, apply the operation, and push the result back on the stack.
                    case Opcode.UNARY_POSITIVE: // Implements TOS = +TOS.
                        throw new NotImplementedException("UNARY_POSITIVE");
                        break;

                    case Opcode.UNARY_NEGATIVE: // Implements TOS = -TOS.
                        throw new NotImplementedException("UNARY_NEGATIVE");
                        break;

                    case Opcode.UNARY_NOT: // Implements TOS = not TOS.
                        throw new NotImplementedException("UNARY_NOT");
                        break;

                    case Opcode.UNARY_CONVERT: // Implements TOS = `TOS`.
                        throw new NotImplementedException("UNARY_CONVERT");
                        break;

                    case Opcode.UNARY_INVERT: // Implements TOS = ~TOS.
                        throw new NotImplementedException("UNARY_INVERT");
                        break;

                    // Binary operations remove the top of the stack (TOS) and the second top-most stack item (TOS1) from the stack. They perform the operation, and put the result back on the stack.
                    case Opcode.BINARY_POWER: // Implements TOS = TOS1 ** TOS.
                        throw new NotImplementedException("BINARY_POWER");
                        break;

                    case Opcode.BINARY_MULTIPLY: // Implements TOS = TOS1 * TOS.
                        throw new NotImplementedException("BINARY_MULTIPLE");
                        break;

                    case Opcode.BINARY_DIVIDE: // Implements TOS = TOS1 / TOS when from __future__ import division is not in effect.
                        throw new NotImplementedException("BINARY_DIVIDE");
                        break;

                    case Opcode.BINARY_TRUE_DIVIDE: // Implements TOS = TOS1 / TOS when from __future__ import division is in effect.
                        throw new NotImplementedException("BINARY_TRUE_DIVIDE");
                        break;

                    case Opcode.BINARY_FLOOR_DIVIDE: // Implements TOS = TOS1 // TOS.
                        throw new NotImplementedException("BINARY_FLOOR_DIVIDE");
                        break;

                    case Opcode.BINARY_MODULO: // Implements TOS = TOS1 % TOS.
                        throw new NotImplementedException("BINARY_MODULO");
                        break;

                    case Opcode.BINARY_ADD: // Implements TOS = TOS1 + TOS.
                        throw new NotImplementedException("BINARY_ADD");
                        break;

                    case Opcode.BINARY_SUBTRACT: // Implements TOS = TOS1 - TOS.
                        throw new NotImplementedException("BINARY_SUBTRACT");
                        break;

                    case Opcode.BINARY_SUBSCR: // Implements TOS = TOS1[TOS].
                        throw new NotImplementedException("BINARY_SUBSCR");
                        break;

                    case Opcode.BINARY_LSHIFT: // Implements TOS = TOS1 << TOS.
                        throw new NotImplementedException("BINARY_LSHIFT");
                        break;

                    case Opcode.BINARY_RSHIFT: // Implements TOS = TOS1 >> TOS.
                        throw new NotImplementedException("BINARY_RSHIFT");
                        break;

                    case Opcode.BINARY_AND: // Implements TOS = TOS1 & TOS.
                        throw new NotImplementedException("BINARY_AND");
                        break;

                    case Opcode.BINARY_XOR: // Implements TOS = TOS1 ^ TOS.
                        throw new NotImplementedException("BINARY_XOR");
                        break;

                    case Opcode.BINARY_OR: // Implements TOS = TOS1 | TOS.
                        throw new NotImplementedException("BINARY_OR");
                        break;

                    case Opcode.LIST_APPEND/*(i)*/:  // Calls list.append(TOS[-i], TOS). Used to implement list comprehensions. While the appended value is popped off, the list object remains on the stack so that it is available for further iterations of the loop.
                        {
                            object b = stack.Pop();
                            List a = stack.Pop() as List;
                            a.append(b);
                            stack.Push(a);
                        }
                        break;

                    case Opcode.INPLACE_POWER: // Implements in-place TOS = TOS1 ** TOS.
                        throw new NotImplementedException("INPLACE_POWER");
                        break;

                    case Opcode.INPLACE_MULTIPLY: // Implements in-place TOS = TOS1 * TOS.
                        throw new NotImplementedException("INPLACE_MULTIPLE");
                        break;

                    case Opcode.INPLACE_DIVIDE: // Implements in-place TOS = TOS1 / TOS when from __future__ import division is not in effect.
                        throw new NotImplementedException("INPLACE_DIVIDE");
                        break;

                    case Opcode.INPLACE_TRUE_DIVIDE: // Implements in-place TOS = TOS1 / TOS when from __future__ import division is in effect.
                        throw new NotImplementedException("INPLACE_TRUE_DIVIDE");
                        break;

                    case Opcode.INPLACE_FLOOR_DIVIDE: // Implements in-place TOS = TOS1 // TOS.
                        throw new NotImplementedException("INPLACE_FLOOR_DIVIDE");
                        break;

                    case Opcode.INPLACE_MODULO: // Implements in-place TOS = TOS1 % TOS.
                        throw new NotImplementedException("INPLACE_MODULO");
                        break;

                    case Opcode.INPLACE_ADD: // Implements in-place TOS = TOS1 + TOS.
                        throw new NotImplementedException("INPLACE_ADD");
                        break;

                    case Opcode.INPLACE_SUBTRACT: // Implements in-place TOS = TOS1 - TOS.
                        throw new NotImplementedException("INPLACE_SUBTRACT");
                        break;

                    case Opcode.INPLACE_LSHIFT: // Implements in-place TOS = TOS1 << TOS.
                        throw new NotImplementedException("INPLACE_LSHIFT");
                        break;

                    case Opcode.INPLACE_RSHIFT: // Implements in-place TOS = TOS1 >> TOS.
                        throw new NotImplementedException("INPLACE_RSHIFT");
                        break;

                    case Opcode.INPLACE_AND: // Implements in-place TOS = TOS1 & TOS.
                        throw new NotImplementedException("INPLACE_AND");
                        break;

                    case Opcode.INPLACE_XOR: // Implements in-place TOS = TOS1 ^ TOS.
                        throw new NotImplementedException("INPLACE_XOR");
                        break;

                    case Opcode.INPLACE_OR: // Implements in-place TOS = TOS1 | TOS.
                        throw new NotImplementedException("INPLACE_OR");
                        break;

                    // The slice opcodes take up to three parameters.
                    case Opcode.SLICE + 0: // Implements TOS = TOS[:].
                    case Opcode.SLICE + 1: // Implements TOS = TOS1[TOS:].
                    case Opcode.SLICE + 2: // Implements TOS = TOS1[:TOS].
                    case Opcode.SLICE + 3: // Implements TOS = TOS2[TOS1: TOS].
                        throw new NotImplementedException("SLICE");
                        break;

                    // Slice assignment needs even an additional parameter. As any statement, they put nothing on the stack.
                    case Opcode.STORE_SLICE + 0: // Implements TOS[:] = TOS1.
                    case Opcode.STORE_SLICE + 1: // Implements TOS1[TOS:] = TOS2.
                    case Opcode.STORE_SLICE + 2: // Implements TOS1[:TOS] = TOS2.
                    case Opcode.STORE_SLICE + 3: // Implements TOS2[TOS1:TOS] = TOS3.
                        throw new NotImplementedException("STORE_SLICE");
                        break;

                    case Opcode.DELETE_SLICE + 0: // Implements del TOS[:].
                    case Opcode.DELETE_SLICE + 1: // Implements del TOS1[TOS:].
                    case Opcode.DELETE_SLICE + 2: // Implements del TOS1[:TOS].
                    case Opcode.DELETE_SLICE + 3: // Implements del TOS2[TOS1:TOS].
                        throw new NotImplementedException("DELETE_SLICE");
                        break;

                    case Opcode.STORE_SUBSCR:  // Implements TOS1[TOS] = TOS2.
                        {
                            object key = stack.Pop();
                            object obj = stack.Pop();
                            object value = stack.Pop();
                            PythonOps.Invoke(context.GlobalContext, obj, "__setitem__", key, value);
                        }
                        break;
                        
                    case Opcode.DELETE_SUBSCR: // Implements del TOS1[TOS].
                        {
                            object key = stack.Pop();
                            object obj = stack.Pop();
                            PythonOps.Invoke(context.GlobalContext, obj, "__delitem__", key);
                        }
                        break;
                        

                    case Opcode.PRINT_EXPR: // Implements the expression statement for the interactive mode. TOS is removed from the stack and printed. In non-interactive mode, an expression statement is terminated with POP_TOP.
                        throw new NotImplementedException("PRINT_EXPR");
                        break;

                    case Opcode.PRINT_ITEM_TO: // Like PRINT_ITEM, but prints the item second from TOS to the file - like object at TOS.This is used by the extended print statement.
                        PythonOps.PrintCommaWithDest (context.GlobalContext, stack.Pop (), stack.Pop ());
                        break;

                    case Opcode.PRINT_ITEM: // Prints TOS to the file-like object bound to sys.stdout. There is one such instruction for each item in the print statement.
                        PythonOps.Print (context.GlobalContext, stack.Pop ());
                        break;
                         
                    case Opcode.PRINT_NEWLINE_TO: // Like PRINT_NEWLINE, but prints the new line on the file-like object on the TOS. This is used by the extended print statement.
                        PythonOps.PrintNewlineWithDest (context.GlobalContext, stack.Pop ());
                        break;

                    case Opcode.PRINT_NEWLINE: // Prints a new line on sys.stdout. This is generated as the last operation of a print statement, unless the statement ends with a comma.
                        PythonOps.PrintNewline (context.GlobalContext);
                        break;

                    case Opcode.RAISE_VARARGS/*(argc)*/: // Raises an exception. argc indicates the number of parameters to the raise statement, ranging from 0 to 3. The handler will find the traceback as TOS2, the parameter as TOS1, and the exception as TOS.
                        throw new NotImplementedException("RAISE_VARARGS");
                        break;

                    case Opcode.LOAD_LOCALS: // Pushes a reference to the locals of the current scope on the stack. This is used in the code for a class definition: After the class body is evaluated, the locals are passed to the class definition.
                        throw new NotImplementedException("LOAD_LOCALS");
                        break;

                    case Opcode.RETURN_VALUE: // Returns with TOS to the caller of the function.
                        retval = stack.Pop ();
                        break;

                    case Opcode.YIELD_VALUE: // Pops TOS and yields it from a generator.
                        throw new NotImplementedException("YIELD_VALUE");
                        break;

                    case Opcode.EXEC_STMT: // Implements exec TOS2,TOS1,TOS. The compiler fills missing optional parameters with None.
                        throw new NotImplementedException("EXEC_STMT");
                        break;

                    case Opcode.POP_BLOCK: // Removes one block from the block stack. Per frame, there is a stack of blocks, denoting nested loops, try statements, and such.
                        throw new NotImplementedException("POP_BLOCK");
                        break;

                    case Opcode.END_FINALLY: // Terminates a finally clause. The interpreter recalls whether the exception has to be re-raised, or whether the function returns, and continues with the outer-next block.
                        throw new NotImplementedException("END_FINALLY");
                        break;

                    case Opcode.BUILD_CLASS: // Creates a new class object. TOS is the methods dictionary, TOS1 the tuple of the names of the base classes, and TOS2 the class name.
                        throw new NotImplementedException("BUILD_CLASS");
                        break;

                    case Opcode.STORE_NAME/*(namei)*/: // Implements name = TOS. namei is the index of name in the attribute co_names of the code object. The compiler tries to use STORE_FAST or STORE_GLOBAL if possible.
                        PythonOps.SetLocal (context.GlobalContext, co_names[oparg], stack.Pop ());
                        break;

                    case Opcode.DELETE_NAME/*(namei)*/: // Implements del name, where namei is the index into co_names attribute of the code object.
                        PythonOps.DeleteLocal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.UNPACK_SEQUENCE/*(count)*/: // Unpacks TOS into count individual values, which are put onto the stack right-to-left.
                        throw new NotImplementedException("UNPACK_SEQUENCE");
                        break;

                    case Opcode.STORE_ATTR/*(namei)*/: // Implements TOS.name = TOS1, where namei is the index of name in co_names.
                        {
                            object obj = stack.Pop();
                            object v = stack.Pop();
                            PythonOps.SetAttr(context.GlobalContext, obj, co_names[oparg], v);
                        }
                        break;
                        
                    case Opcode.DELETE_ATTR/*(namei)*/: // Implements del TOS.name, using namei as index into co_names.
                        PythonOps.DeleteAttr (context.GlobalContext, stack.Pop (), co_names[oparg]);
                        break;

                    case Opcode.STORE_GLOBAL/*(namei)*/: // Works as STORE_NAME, but stores the name as a global.
                        PythonOps.SetGlobal (context.GlobalContext, co_names[oparg], stack.Pop ());
                        break;

                    case Opcode.DELETE_GLOBAL/*(namei)*/: // Works as DELETE_NAME, but deletes a global name.
                        PythonOps.DeleteGlobal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.LOAD_NAME/*(namei)*/: // Pushes the value associated with co_names[namei] onto the stack.
                        PythonOps.GetLocal (context.GlobalContext, co_names[oparg]);
                        break;

                    case Opcode.LOAD_GLOBAL/*(namei)*/: // Loads the global named co_names[namei] onto the stack.
                        stack.Push (PythonOps.GetGlobal (context.GlobalContext, co_names[oparg]));
                        break;

                    case Opcode.DELETE_FAST/*(var_num)*/: // Deletes local co_varnames[var_num].
                        throw new NotImplementedException("DELETE_FAST");
                        break;

                    case Opcode.LOAD_CLOSURE/*(i)*/: // Pushes a reference to the cell contained in slot i of the cell and free variable storage. The name of the variable is co_cellvars[i] if i is less than the length of co_cellvars. Otherwise it is co_freevars[i - len(co_cellvars)].
                        throw new NotImplementedException("LOAD_CLOSURE");
                        break;

                    case Opcode.LOAD_DEREF/*(i)*/: // Loads the cell contained in slot i of the cell and free variable storage. Pushes a reference to the object the cell contains on the stack.
                        throw new NotImplementedException("LOAD_DEREF");
                        break;

                    case Opcode.STORE_DEREF/*(i)*/: // Stores TOS into the cell contained in slot i of the cell and free variable storage.
                        throw new NotImplementedException("STORE_DEREF");
                        break;

                    case Opcode.BUILD_TUPLE/*(count)*/: // Creates a tuple consuming count items from the stack, and pushes the resulting tuple onto the stack.
                        {
                            List l = new List();
                            for (int i = 0; i < oparg; i++) {
                                l.Add(stack.Pop());
                            }
                            stack.Push(PythonOps.MakeTupleFromSequence(l));
                        }
                        break;

                    case Opcode.BUILD_LIST/*(count)*/: // Works as BUILD_TUPLE, but creates a list. 
                        {
                            List l = new List();
                            for (int i = 0; i < oparg; i++) {
                                l.Add(stack.Pop());
                            }
                            stack.Push(l);
                        }
                        break;

                    case Opcode.BUILD_SET/*(count)*/: // Works as BUILD_TUPLE, but creates a set.
                        {
                            SetCollection s = PythonOps.MakeSet();
                            for (int i = 0; i < oparg; i++) {
                                s.add(stack.Pop());
                            }
                            stack.Push(s);                            
                        }
                        break;

                    case Opcode.BUILD_MAP/*(count)*/: // Pushes a new dictionary object onto the stack. The dictionary is pre-sized to hold count entries.
                        stack.Push (new PythonDictionary (oparg));
                        break;

                    case Opcode.LOAD_ATTR/*(namei)*/: // Replaces TOS with getattr(TOS, co_names[namei]).
                        stack.Push (PythonOps.ObjectGetAttribute (context.GlobalContext, stack.Pop (), co_names[oparg]));
                        break;

                    case Opcode.COMPARE_OP/*(opname)*/: // Performs a Boolean operation. The operation name can be found in cmp_op[opname].
                        throw new NotImplementedException("COMPARE_OP");
                        break;

                    case Opcode.IMPORT_NAME/*(namei)*/: // Imports the module co_names[namei]. TOS and TOS1 are popped and provide the fromlist and level arguments of __import__(). The module object is pushed onto the stack. The current namespace is not affected: for a proper import statement, a subsequent STORE_FAST instruction modifies the namespace.
                        throw new NotImplementedException("IMPORT_NAME");
                        break;

                    case Opcode.IMPORT_STAR: // Loads all symbols not starting with '_' directly from the module TOS to the local namespace. The module is popped after loading all names. This opcode implements from module import *.
                        throw new NotImplementedException("IMPORT_STAR");
                        break;

                    case Opcode.IMPORT_FROM/*(namei)*/: // Loads the attribute co_names[namei] from the module found in TOS. The resulting object is pushed onto the stack, to be subsequently stored by a STORE_FAST instruction.
                        throw new NotImplementedException("IMPORT_FROM");
                        break;

                    case Opcode.JUMP_FORWARD/*(delta)*/: // Increments bytecode counter by delta.
                        next_instr += oparg;
                        break;

                    case Opcode.JUMP_IF_FALSE_OR_POP/*(target)*/: // If TOS is false, sets the bytecode counter to target and leaves TOS on the stack. Otherwise (TOS is true), TOS is popped.
                        if (!PythonOps.CheckingConvertToNonZero (stack.Peek ())) {
                            next_instr += oparg;
                        } else {
                            stack.Pop();
                        }
                        break;

                    case Opcode.JUMP_IF_TRUE_OR_POP/*(target)*/: // If TOS is true, sets the bytecode counter to target and leaves TOS on the stack. Otherwise (TOS is false), TOS is popped.
                        if (PythonOps.CheckingConvertToNonZero (stack.Peek ())) {
                            next_instr += oparg;
                        } else {
                            stack.Pop();
                        }
                        break;

                    case Opcode.JUMP_ABSOLUTE/*(target)*/: // Set bytecode counter to target.
                        next_instr = oparg;
                        break;

                    case Opcode.GET_ITER: // Implements TOS = iter(TOS).
                        stack.Push(PythonOps.GetEnumerator(stack.Pop()));
                        break;

                    case Opcode.FOR_ITER/*(delta)*/: // TOS is an iterator. Call its next() method. If this yields a new value, push it on the stack (leaving the iterator below it). If the iterator indicates it is exhausted TOS is popped, and the bytecode counter is incremented by delta.
                        {
                            //object tos = stack.Peek();
                            
                            throw new NotImplementedException("FOR_ITER");
                        }
                        break;

                    case Opcode.BREAK_LOOP: // Terminates a loop due to a break statement.
                        throw new NotImplementedException("BREAK_LOOP");
                        break;

                    case Opcode.CONTINUE_LOOP/*(target)*/: // Continues a loop due to a continue statement. target is the address to jump to (which should be a FOR_ITER instruction).
                        throw new NotImplementedException("CONTINUE_LOOP");
                        break;

                    case Opcode.SETUP_LOOP/*(delta)*/: // Pushes a block for a loop onto the block stack. The block spans from the current instruction with a size of delta bytes.
                        throw new NotImplementedException("SETUP_LOOP");
                    case Opcode.SETUP_EXCEPT/*(delta)*/: // Pushes a try block from a try-except clause onto the block stack. delta points to the first except block.
                        throw new NotImplementedException("SETUP_EXCEPT");
                    case Opcode.SETUP_FINALLY/*(delta)*/: // Pushes a try block from a try-except clause onto the block stack. delta points to the finally block.
                        throw new NotImplementedException("SETUP_FINALLY");
                        break;

                    case Opcode.WITH_CLEANUP: // Cleans up the stack when a with statement block exits. On top of the stack are 1–3 values indicating how/why the finally clause was entered:
                        /* 
                        TOP = None
                        (TOP, SECOND) = (WHY_{RETURN,CONTINUE}), retval
                        TOP = WHY_*; no retval below it
                        (TOP, SECOND, THIRD) = exc_info()
                        Under them is EXIT, the context manager’s __exit__() bound method.

                        In the last case, EXIT(TOP, SECOND, THIRD) is called, otherwise EXIT(None, None, None).

                        EXIT is removed from the stack, leaving the values above it in the same order. In addition, if the stack represents an exception, and the function call returns a ‘true’ value, this information is “zapped”, to prevent END_FINALLY from re-raising the exception. (But non-local gotos should still be resumed.)
                        */
                        throw new NotImplementedException("WITH_CLEANUP");
                        break;

                    case Opcode.CALL_FUNCTION/*(argc)*/:  // Calls a function. The low byte of argc indicates the number of positional parameters, the high byte the number of keyword parameters. On the stack, the opcode finds the keyword parameters first. For each keyword argument, the value is on top of the key. Below the keyword parameters, the positional parameters are on the stack, with the right-most parameter on top. Below the parameters, the function object to call is on the stack. Pops all function arguments, and the function itself off the stack, and pushes the return value.
                        throw new NotImplementedException("CALL_FUNCTION");
                        break;

                    case Opcode.CALL_FUNCTION_VAR/*(argc)*/: // Calls a function. argc is interpreted as in CALL_FUNCTION. The top element on the stack contains the variable argument list, followed by keyword and positional arguments.
                        throw new NotImplementedException("CALL_FUNCTION_VAR");
                    case Opcode.CALL_FUNCTION_KW/*(argc)*/: // Calls a function. argc is interpreted as in CALL_FUNCTION. The top element on the stack contains the keyword arguments dictionary, followed by explicit keyword and positional arguments.
                        throw new NotImplementedException("CALL_FUNCTION_KW");
                    case Opcode.CALL_FUNCTION_VAR_KW/*(argc)*/: // Calls a function. argc is interpreted as in CALL_FUNCTION. The top element on the stack contains the keyword arguments dictionary, followed by the variable-arguments tuple, followed by explicit keyword and positional arguments.
                        throw new NotImplementedException("CALL_FUNCTION_VAR_KW");
                        break;

                    case Opcode.MAKE_FUNCTION/*(argc)*/: // Pushes a new function object on the stack. TOS is the code associated with the function. The function object is defined to have argc default parameters, which are found below TOS.
                        throw new NotImplementedException("MAKE_FUNCTION");
                        break;

                    case Opcode.MAKE_CLOSURE/*(argc)*/: // Creates a new function object, sets its func_closure slot, and pushes it on the stack. TOS is the code associated with the function, TOS1 the tuple containing cells for the closure’s free variables. The function also has argc default parameters, which are found below the cells.
                        throw new NotImplementedException("MAKE_CLOSURE");
                        break;

                    case Opcode.BUILD_SLICE/*(argc)*/: // Pushes a slice object on the stack. argc must be 2 or 3. If it is 2, slice(TOS1, TOS) is pushed; if it is 3, slice(TOS2, TOS1, TOS) is pushed. See the slice() built-in function for more information.
                        object step = (oparg == 3) ? stack.Pop () : null;
                        object stop = stack.Pop ();
                        object start = stack.Pop ();
                        stack.Push (PythonOps.MakeSlice (start, stop, step));
                        break;

                    case Opcode.EXTENDED_ARG/*(ext)*/: // Prefixes any opcode which has an argument too big to fit into the default two bytes. ext holds two additional bytes which, taken together with the subsequent opcode’s argument, comprise a four-byte argument, ext being the two most-significant bytes.
                        opcode = co_code[next_instr++];
                        next_instr += 2;
                        oparg = oparg << 16 | ((co_code[next_instr] << 8) + co_code[next_instr - 1]);
                        break;

                    default:
                        // TODO: determine if we want to throw an exception here or something
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

        public PythonTuple co_varnames {
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
