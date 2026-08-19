using System.Reflection.Emit;

// HarmonyX's opcode helper for transpilers: Code.Ldarg_0, Code.Call[operand].
// pardeike Harmony has no equivalent, so a mod that uses it will not even load.
//
// Each entry derives from HarmonyLib.CodeMatch, which pardeike Harmony does have,
// so these are real CodeMatch instances and work anywhere one is accepted. The
// indexer mirrors HarmonyX's, where Code.Call[method] narrows the match to an
// operand. Generated from the opcode list, not written by hand.
namespace HarmonyLib;

public static class Code {
    // Not an opcode: HarmonyX's wildcard, matching any instruction with the given
    // operand.
    public static Operand_ Operand => new();

    public static Add_ Add => new();
    public static Add_Ovf_ Add_Ovf => new();
    public static Add_Ovf_Un_ Add_Ovf_Un => new();
    public static And_ And => new();
    public static Arglist_ Arglist => new();
    public static Beq_ Beq => new();
    public static Beq_S_ Beq_S => new();
    public static Bge_ Bge => new();
    public static Bge_S_ Bge_S => new();
    public static Bge_Un_ Bge_Un => new();
    public static Bge_Un_S_ Bge_Un_S => new();
    public static Bgt_ Bgt => new();
    public static Bgt_S_ Bgt_S => new();
    public static Bgt_Un_ Bgt_Un => new();
    public static Bgt_Un_S_ Bgt_Un_S => new();
    public static Ble_ Ble => new();
    public static Ble_S_ Ble_S => new();
    public static Ble_Un_ Ble_Un => new();
    public static Ble_Un_S_ Ble_Un_S => new();
    public static Blt_ Blt => new();
    public static Blt_S_ Blt_S => new();
    public static Blt_Un_ Blt_Un => new();
    public static Blt_Un_S_ Blt_Un_S => new();
    public static Bne_Un_ Bne_Un => new();
    public static Bne_Un_S_ Bne_Un_S => new();
    public static Box_ Box => new();
    public static Br_ Br => new();
    public static Br_S_ Br_S => new();
    public static Break_ Break => new();
    public static Brfalse_ Brfalse => new();
    public static Brfalse_S_ Brfalse_S => new();
    public static Brtrue_ Brtrue => new();
    public static Brtrue_S_ Brtrue_S => new();
    public static Call_ Call => new();
    public static Calli_ Calli => new();
    public static Callvirt_ Callvirt => new();
    public static Castclass_ Castclass => new();
    public static Ceq_ Ceq => new();
    public static Cgt_ Cgt => new();
    public static Cgt_Un_ Cgt_Un => new();
    public static Ckfinite_ Ckfinite => new();
    public static Clt_ Clt => new();
    public static Clt_Un_ Clt_Un => new();
    public static Constrained_ Constrained => new();
    public static Conv_I_ Conv_I => new();
    public static Conv_I1_ Conv_I1 => new();
    public static Conv_I2_ Conv_I2 => new();
    public static Conv_I4_ Conv_I4 => new();
    public static Conv_I8_ Conv_I8 => new();
    public static Conv_Ovf_I_ Conv_Ovf_I => new();
    public static Conv_Ovf_I1_ Conv_Ovf_I1 => new();
    public static Conv_Ovf_I1_Un_ Conv_Ovf_I1_Un => new();
    public static Conv_Ovf_I2_ Conv_Ovf_I2 => new();
    public static Conv_Ovf_I2_Un_ Conv_Ovf_I2_Un => new();
    public static Conv_Ovf_I4_ Conv_Ovf_I4 => new();
    public static Conv_Ovf_I4_Un_ Conv_Ovf_I4_Un => new();
    public static Conv_Ovf_I8_ Conv_Ovf_I8 => new();
    public static Conv_Ovf_I8_Un_ Conv_Ovf_I8_Un => new();
    public static Conv_Ovf_I_Un_ Conv_Ovf_I_Un => new();
    public static Conv_Ovf_U_ Conv_Ovf_U => new();
    public static Conv_Ovf_U1_ Conv_Ovf_U1 => new();
    public static Conv_Ovf_U1_Un_ Conv_Ovf_U1_Un => new();
    public static Conv_Ovf_U2_ Conv_Ovf_U2 => new();
    public static Conv_Ovf_U2_Un_ Conv_Ovf_U2_Un => new();
    public static Conv_Ovf_U4_ Conv_Ovf_U4 => new();
    public static Conv_Ovf_U4_Un_ Conv_Ovf_U4_Un => new();
    public static Conv_Ovf_U8_ Conv_Ovf_U8 => new();
    public static Conv_Ovf_U8_Un_ Conv_Ovf_U8_Un => new();
    public static Conv_Ovf_U_Un_ Conv_Ovf_U_Un => new();
    public static Conv_R4_ Conv_R4 => new();
    public static Conv_R8_ Conv_R8 => new();
    public static Conv_R_Un_ Conv_R_Un => new();
    public static Conv_U_ Conv_U => new();
    public static Conv_U1_ Conv_U1 => new();
    public static Conv_U2_ Conv_U2 => new();
    public static Conv_U4_ Conv_U4 => new();
    public static Conv_U8_ Conv_U8 => new();
    public static Cpblk_ Cpblk => new();
    public static Cpobj_ Cpobj => new();
    public static Div_ Div => new();
    public static Div_Un_ Div_Un => new();
    public static Dup_ Dup => new();
    public static Endfilter_ Endfilter => new();
    public static Endfinally_ Endfinally => new();
    public static Initblk_ Initblk => new();
    public static Initobj_ Initobj => new();
    public static Isinst_ Isinst => new();
    public static Jmp_ Jmp => new();
    public static Ldarg_ Ldarg => new();
    public static Ldarg_0_ Ldarg_0 => new();
    public static Ldarg_1_ Ldarg_1 => new();
    public static Ldarg_2_ Ldarg_2 => new();
    public static Ldarg_3_ Ldarg_3 => new();
    public static Ldarg_S_ Ldarg_S => new();
    public static Ldarga_ Ldarga => new();
    public static Ldarga_S_ Ldarga_S => new();
    public static Ldc_I4_ Ldc_I4 => new();
    public static Ldc_I4_0_ Ldc_I4_0 => new();
    public static Ldc_I4_1_ Ldc_I4_1 => new();
    public static Ldc_I4_2_ Ldc_I4_2 => new();
    public static Ldc_I4_3_ Ldc_I4_3 => new();
    public static Ldc_I4_4_ Ldc_I4_4 => new();
    public static Ldc_I4_5_ Ldc_I4_5 => new();
    public static Ldc_I4_6_ Ldc_I4_6 => new();
    public static Ldc_I4_7_ Ldc_I4_7 => new();
    public static Ldc_I4_8_ Ldc_I4_8 => new();
    public static Ldc_I4_M1_ Ldc_I4_M1 => new();
    public static Ldc_I4_S_ Ldc_I4_S => new();
    public static Ldc_I8_ Ldc_I8 => new();
    public static Ldc_R4_ Ldc_R4 => new();
    public static Ldc_R8_ Ldc_R8 => new();
    public static Ldelem_ Ldelem => new();
    public static Ldelem_I_ Ldelem_I => new();
    public static Ldelem_I1_ Ldelem_I1 => new();
    public static Ldelem_I2_ Ldelem_I2 => new();
    public static Ldelem_I4_ Ldelem_I4 => new();
    public static Ldelem_I8_ Ldelem_I8 => new();
    public static Ldelem_R4_ Ldelem_R4 => new();
    public static Ldelem_R8_ Ldelem_R8 => new();
    public static Ldelem_Ref_ Ldelem_Ref => new();
    public static Ldelem_U1_ Ldelem_U1 => new();
    public static Ldelem_U2_ Ldelem_U2 => new();
    public static Ldelem_U4_ Ldelem_U4 => new();
    public static Ldelema_ Ldelema => new();
    public static Ldfld_ Ldfld => new();
    public static Ldflda_ Ldflda => new();
    public static Ldftn_ Ldftn => new();
    public static Ldind_I_ Ldind_I => new();
    public static Ldind_I1_ Ldind_I1 => new();
    public static Ldind_I2_ Ldind_I2 => new();
    public static Ldind_I4_ Ldind_I4 => new();
    public static Ldind_I8_ Ldind_I8 => new();
    public static Ldind_R4_ Ldind_R4 => new();
    public static Ldind_R8_ Ldind_R8 => new();
    public static Ldind_Ref_ Ldind_Ref => new();
    public static Ldind_U1_ Ldind_U1 => new();
    public static Ldind_U2_ Ldind_U2 => new();
    public static Ldind_U4_ Ldind_U4 => new();
    public static Ldlen_ Ldlen => new();
    public static Ldloc_ Ldloc => new();
    public static Ldloc_0_ Ldloc_0 => new();
    public static Ldloc_1_ Ldloc_1 => new();
    public static Ldloc_2_ Ldloc_2 => new();
    public static Ldloc_3_ Ldloc_3 => new();
    public static Ldloc_S_ Ldloc_S => new();
    public static Ldloca_ Ldloca => new();
    public static Ldloca_S_ Ldloca_S => new();
    public static Ldnull_ Ldnull => new();
    public static Ldobj_ Ldobj => new();
    public static Ldsfld_ Ldsfld => new();
    public static Ldsflda_ Ldsflda => new();
    public static Ldstr_ Ldstr => new();
    public static Ldtoken_ Ldtoken => new();
    public static Ldvirtftn_ Ldvirtftn => new();
    public static Leave_ Leave => new();
    public static Leave_S_ Leave_S => new();
    public static Localloc_ Localloc => new();
    public static Mkrefany_ Mkrefany => new();
    public static Mul_ Mul => new();
    public static Mul_Ovf_ Mul_Ovf => new();
    public static Mul_Ovf_Un_ Mul_Ovf_Un => new();
    public static Neg_ Neg => new();
    public static Newarr_ Newarr => new();
    public static Newobj_ Newobj => new();
    public static Nop_ Nop => new();
    public static Not_ Not => new();
    public static Or_ Or => new();
    public static Pop_ Pop => new();
    public static Prefix1_ Prefix1 => new();
    public static Prefix2_ Prefix2 => new();
    public static Prefix3_ Prefix3 => new();
    public static Prefix4_ Prefix4 => new();
    public static Prefix5_ Prefix5 => new();
    public static Prefix6_ Prefix6 => new();
    public static Prefix7_ Prefix7 => new();
    public static Prefixref_ Prefixref => new();
    public static Readonly_ Readonly => new();
    public static Refanytype_ Refanytype => new();
    public static Refanyval_ Refanyval => new();
    public static Rem_ Rem => new();
    public static Rem_Un_ Rem_Un => new();
    public static Ret_ Ret => new();
    public static Rethrow_ Rethrow => new();
    public static Shl_ Shl => new();
    public static Shr_ Shr => new();
    public static Shr_Un_ Shr_Un => new();
    public static Sizeof_ Sizeof => new();
    public static Starg_ Starg => new();
    public static Starg_S_ Starg_S => new();
    public static Stelem_ Stelem => new();
    public static Stelem_I_ Stelem_I => new();
    public static Stelem_I1_ Stelem_I1 => new();
    public static Stelem_I2_ Stelem_I2 => new();
    public static Stelem_I4_ Stelem_I4 => new();
    public static Stelem_I8_ Stelem_I8 => new();
    public static Stelem_R4_ Stelem_R4 => new();
    public static Stelem_R8_ Stelem_R8 => new();
    public static Stelem_Ref_ Stelem_Ref => new();
    public static Stfld_ Stfld => new();
    public static Stind_I_ Stind_I => new();
    public static Stind_I1_ Stind_I1 => new();
    public static Stind_I2_ Stind_I2 => new();
    public static Stind_I4_ Stind_I4 => new();
    public static Stind_I8_ Stind_I8 => new();
    public static Stind_R4_ Stind_R4 => new();
    public static Stind_R8_ Stind_R8 => new();
    public static Stind_Ref_ Stind_Ref => new();
    public static Stloc_ Stloc => new();
    public static Stloc_0_ Stloc_0 => new();
    public static Stloc_1_ Stloc_1 => new();
    public static Stloc_2_ Stloc_2 => new();
    public static Stloc_3_ Stloc_3 => new();
    public static Stloc_S_ Stloc_S => new();
    public static Stobj_ Stobj => new();
    public static Stsfld_ Stsfld => new();
    public static Sub_ Sub => new();
    public static Sub_Ovf_ Sub_Ovf => new();
    public static Sub_Ovf_Un_ Sub_Ovf_Un => new();
    public static Switch_ Switch => new();
    public static Tailcall_ Tailcall => new();
    public static Throw_ Throw => new();
    public static Unaligned_ Unaligned => new();
    public static Unbox_ Unbox => new();
    public static Unbox_Any_ Unbox_Any => new();
    public static Volatile_ Volatile => new();
    public static Xor_ Xor => new();

    public class Add_ : CodeMatch {
        public Add_() : base(OpCodes.Add, null, null) { }
        private Add_(object operand, string name) : base(OpCodes.Add, operand, name) { }
        public Add_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Add_Ovf_ : CodeMatch {
        public Add_Ovf_() : base(OpCodes.Add_Ovf, null, null) { }
        private Add_Ovf_(object operand, string name) : base(OpCodes.Add_Ovf, operand, name) { }
        public Add_Ovf_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Add_Ovf_Un_ : CodeMatch {
        public Add_Ovf_Un_() : base(OpCodes.Add_Ovf_Un, null, null) { }
        private Add_Ovf_Un_(object operand, string name) : base(OpCodes.Add_Ovf_Un, operand, name) { }
        public Add_Ovf_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class And_ : CodeMatch {
        public And_() : base(OpCodes.And, null, null) { }
        private And_(object operand, string name) : base(OpCodes.And, operand, name) { }
        public And_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Arglist_ : CodeMatch {
        public Arglist_() : base(OpCodes.Arglist, null, null) { }
        private Arglist_(object operand, string name) : base(OpCodes.Arglist, operand, name) { }
        public Arglist_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Beq_ : CodeMatch {
        public Beq_() : base(OpCodes.Beq, null, null) { }
        private Beq_(object operand, string name) : base(OpCodes.Beq, operand, name) { }
        public Beq_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Beq_S_ : CodeMatch {
        public Beq_S_() : base(OpCodes.Beq_S, null, null) { }
        private Beq_S_(object operand, string name) : base(OpCodes.Beq_S, operand, name) { }
        public Beq_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bge_ : CodeMatch {
        public Bge_() : base(OpCodes.Bge, null, null) { }
        private Bge_(object operand, string name) : base(OpCodes.Bge, operand, name) { }
        public Bge_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bge_S_ : CodeMatch {
        public Bge_S_() : base(OpCodes.Bge_S, null, null) { }
        private Bge_S_(object operand, string name) : base(OpCodes.Bge_S, operand, name) { }
        public Bge_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bge_Un_ : CodeMatch {
        public Bge_Un_() : base(OpCodes.Bge_Un, null, null) { }
        private Bge_Un_(object operand, string name) : base(OpCodes.Bge_Un, operand, name) { }
        public Bge_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bge_Un_S_ : CodeMatch {
        public Bge_Un_S_() : base(OpCodes.Bge_Un_S, null, null) { }
        private Bge_Un_S_(object operand, string name) : base(OpCodes.Bge_Un_S, operand, name) { }
        public Bge_Un_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bgt_ : CodeMatch {
        public Bgt_() : base(OpCodes.Bgt, null, null) { }
        private Bgt_(object operand, string name) : base(OpCodes.Bgt, operand, name) { }
        public Bgt_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bgt_S_ : CodeMatch {
        public Bgt_S_() : base(OpCodes.Bgt_S, null, null) { }
        private Bgt_S_(object operand, string name) : base(OpCodes.Bgt_S, operand, name) { }
        public Bgt_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bgt_Un_ : CodeMatch {
        public Bgt_Un_() : base(OpCodes.Bgt_Un, null, null) { }
        private Bgt_Un_(object operand, string name) : base(OpCodes.Bgt_Un, operand, name) { }
        public Bgt_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bgt_Un_S_ : CodeMatch {
        public Bgt_Un_S_() : base(OpCodes.Bgt_Un_S, null, null) { }
        private Bgt_Un_S_(object operand, string name) : base(OpCodes.Bgt_Un_S, operand, name) { }
        public Bgt_Un_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ble_ : CodeMatch {
        public Ble_() : base(OpCodes.Ble, null, null) { }
        private Ble_(object operand, string name) : base(OpCodes.Ble, operand, name) { }
        public Ble_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ble_S_ : CodeMatch {
        public Ble_S_() : base(OpCodes.Ble_S, null, null) { }
        private Ble_S_(object operand, string name) : base(OpCodes.Ble_S, operand, name) { }
        public Ble_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ble_Un_ : CodeMatch {
        public Ble_Un_() : base(OpCodes.Ble_Un, null, null) { }
        private Ble_Un_(object operand, string name) : base(OpCodes.Ble_Un, operand, name) { }
        public Ble_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ble_Un_S_ : CodeMatch {
        public Ble_Un_S_() : base(OpCodes.Ble_Un_S, null, null) { }
        private Ble_Un_S_(object operand, string name) : base(OpCodes.Ble_Un_S, operand, name) { }
        public Ble_Un_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Blt_ : CodeMatch {
        public Blt_() : base(OpCodes.Blt, null, null) { }
        private Blt_(object operand, string name) : base(OpCodes.Blt, operand, name) { }
        public Blt_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Blt_S_ : CodeMatch {
        public Blt_S_() : base(OpCodes.Blt_S, null, null) { }
        private Blt_S_(object operand, string name) : base(OpCodes.Blt_S, operand, name) { }
        public Blt_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Blt_Un_ : CodeMatch {
        public Blt_Un_() : base(OpCodes.Blt_Un, null, null) { }
        private Blt_Un_(object operand, string name) : base(OpCodes.Blt_Un, operand, name) { }
        public Blt_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Blt_Un_S_ : CodeMatch {
        public Blt_Un_S_() : base(OpCodes.Blt_Un_S, null, null) { }
        private Blt_Un_S_(object operand, string name) : base(OpCodes.Blt_Un_S, operand, name) { }
        public Blt_Un_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bne_Un_ : CodeMatch {
        public Bne_Un_() : base(OpCodes.Bne_Un, null, null) { }
        private Bne_Un_(object operand, string name) : base(OpCodes.Bne_Un, operand, name) { }
        public Bne_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Bne_Un_S_ : CodeMatch {
        public Bne_Un_S_() : base(OpCodes.Bne_Un_S, null, null) { }
        private Bne_Un_S_(object operand, string name) : base(OpCodes.Bne_Un_S, operand, name) { }
        public Bne_Un_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Box_ : CodeMatch {
        public Box_() : base(OpCodes.Box, null, null) { }
        private Box_(object operand, string name) : base(OpCodes.Box, operand, name) { }
        public Box_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Br_ : CodeMatch {
        public Br_() : base(OpCodes.Br, null, null) { }
        private Br_(object operand, string name) : base(OpCodes.Br, operand, name) { }
        public Br_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Br_S_ : CodeMatch {
        public Br_S_() : base(OpCodes.Br_S, null, null) { }
        private Br_S_(object operand, string name) : base(OpCodes.Br_S, operand, name) { }
        public Br_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Break_ : CodeMatch {
        public Break_() : base(OpCodes.Break, null, null) { }
        private Break_(object operand, string name) : base(OpCodes.Break, operand, name) { }
        public Break_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Brfalse_ : CodeMatch {
        public Brfalse_() : base(OpCodes.Brfalse, null, null) { }
        private Brfalse_(object operand, string name) : base(OpCodes.Brfalse, operand, name) { }
        public Brfalse_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Brfalse_S_ : CodeMatch {
        public Brfalse_S_() : base(OpCodes.Brfalse_S, null, null) { }
        private Brfalse_S_(object operand, string name) : base(OpCodes.Brfalse_S, operand, name) { }
        public Brfalse_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Brtrue_ : CodeMatch {
        public Brtrue_() : base(OpCodes.Brtrue, null, null) { }
        private Brtrue_(object operand, string name) : base(OpCodes.Brtrue, operand, name) { }
        public Brtrue_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Brtrue_S_ : CodeMatch {
        public Brtrue_S_() : base(OpCodes.Brtrue_S, null, null) { }
        private Brtrue_S_(object operand, string name) : base(OpCodes.Brtrue_S, operand, name) { }
        public Brtrue_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Call_ : CodeMatch {
        public Call_() : base(OpCodes.Call, null, null) { }
        private Call_(object operand, string name) : base(OpCodes.Call, operand, name) { }
        public Call_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Calli_ : CodeMatch {
        public Calli_() : base(OpCodes.Calli, null, null) { }
        private Calli_(object operand, string name) : base(OpCodes.Calli, operand, name) { }
        public Calli_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Callvirt_ : CodeMatch {
        public Callvirt_() : base(OpCodes.Callvirt, null, null) { }
        private Callvirt_(object operand, string name) : base(OpCodes.Callvirt, operand, name) { }
        public Callvirt_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Castclass_ : CodeMatch {
        public Castclass_() : base(OpCodes.Castclass, null, null) { }
        private Castclass_(object operand, string name) : base(OpCodes.Castclass, operand, name) { }
        public Castclass_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ceq_ : CodeMatch {
        public Ceq_() : base(OpCodes.Ceq, null, null) { }
        private Ceq_(object operand, string name) : base(OpCodes.Ceq, operand, name) { }
        public Ceq_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Cgt_ : CodeMatch {
        public Cgt_() : base(OpCodes.Cgt, null, null) { }
        private Cgt_(object operand, string name) : base(OpCodes.Cgt, operand, name) { }
        public Cgt_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Cgt_Un_ : CodeMatch {
        public Cgt_Un_() : base(OpCodes.Cgt_Un, null, null) { }
        private Cgt_Un_(object operand, string name) : base(OpCodes.Cgt_Un, operand, name) { }
        public Cgt_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ckfinite_ : CodeMatch {
        public Ckfinite_() : base(OpCodes.Ckfinite, null, null) { }
        private Ckfinite_(object operand, string name) : base(OpCodes.Ckfinite, operand, name) { }
        public Ckfinite_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Clt_ : CodeMatch {
        public Clt_() : base(OpCodes.Clt, null, null) { }
        private Clt_(object operand, string name) : base(OpCodes.Clt, operand, name) { }
        public Clt_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Clt_Un_ : CodeMatch {
        public Clt_Un_() : base(OpCodes.Clt_Un, null, null) { }
        private Clt_Un_(object operand, string name) : base(OpCodes.Clt_Un, operand, name) { }
        public Clt_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Constrained_ : CodeMatch {
        public Constrained_() : base(OpCodes.Constrained, null, null) { }
        private Constrained_(object operand, string name) : base(OpCodes.Constrained, operand, name) { }
        public Constrained_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_I_ : CodeMatch {
        public Conv_I_() : base(OpCodes.Conv_I, null, null) { }
        private Conv_I_(object operand, string name) : base(OpCodes.Conv_I, operand, name) { }
        public Conv_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_I1_ : CodeMatch {
        public Conv_I1_() : base(OpCodes.Conv_I1, null, null) { }
        private Conv_I1_(object operand, string name) : base(OpCodes.Conv_I1, operand, name) { }
        public Conv_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_I2_ : CodeMatch {
        public Conv_I2_() : base(OpCodes.Conv_I2, null, null) { }
        private Conv_I2_(object operand, string name) : base(OpCodes.Conv_I2, operand, name) { }
        public Conv_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_I4_ : CodeMatch {
        public Conv_I4_() : base(OpCodes.Conv_I4, null, null) { }
        private Conv_I4_(object operand, string name) : base(OpCodes.Conv_I4, operand, name) { }
        public Conv_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_I8_ : CodeMatch {
        public Conv_I8_() : base(OpCodes.Conv_I8, null, null) { }
        private Conv_I8_(object operand, string name) : base(OpCodes.Conv_I8, operand, name) { }
        public Conv_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I_ : CodeMatch {
        public Conv_Ovf_I_() : base(OpCodes.Conv_Ovf_I, null, null) { }
        private Conv_Ovf_I_(object operand, string name) : base(OpCodes.Conv_Ovf_I, operand, name) { }
        public Conv_Ovf_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I1_ : CodeMatch {
        public Conv_Ovf_I1_() : base(OpCodes.Conv_Ovf_I1, null, null) { }
        private Conv_Ovf_I1_(object operand, string name) : base(OpCodes.Conv_Ovf_I1, operand, name) { }
        public Conv_Ovf_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I1_Un_ : CodeMatch {
        public Conv_Ovf_I1_Un_() : base(OpCodes.Conv_Ovf_I1_Un, null, null) { }
        private Conv_Ovf_I1_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_I1_Un, operand, name) { }
        public Conv_Ovf_I1_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I2_ : CodeMatch {
        public Conv_Ovf_I2_() : base(OpCodes.Conv_Ovf_I2, null, null) { }
        private Conv_Ovf_I2_(object operand, string name) : base(OpCodes.Conv_Ovf_I2, operand, name) { }
        public Conv_Ovf_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I2_Un_ : CodeMatch {
        public Conv_Ovf_I2_Un_() : base(OpCodes.Conv_Ovf_I2_Un, null, null) { }
        private Conv_Ovf_I2_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_I2_Un, operand, name) { }
        public Conv_Ovf_I2_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I4_ : CodeMatch {
        public Conv_Ovf_I4_() : base(OpCodes.Conv_Ovf_I4, null, null) { }
        private Conv_Ovf_I4_(object operand, string name) : base(OpCodes.Conv_Ovf_I4, operand, name) { }
        public Conv_Ovf_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I4_Un_ : CodeMatch {
        public Conv_Ovf_I4_Un_() : base(OpCodes.Conv_Ovf_I4_Un, null, null) { }
        private Conv_Ovf_I4_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_I4_Un, operand, name) { }
        public Conv_Ovf_I4_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I8_ : CodeMatch {
        public Conv_Ovf_I8_() : base(OpCodes.Conv_Ovf_I8, null, null) { }
        private Conv_Ovf_I8_(object operand, string name) : base(OpCodes.Conv_Ovf_I8, operand, name) { }
        public Conv_Ovf_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I8_Un_ : CodeMatch {
        public Conv_Ovf_I8_Un_() : base(OpCodes.Conv_Ovf_I8_Un, null, null) { }
        private Conv_Ovf_I8_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_I8_Un, operand, name) { }
        public Conv_Ovf_I8_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_I_Un_ : CodeMatch {
        public Conv_Ovf_I_Un_() : base(OpCodes.Conv_Ovf_I_Un, null, null) { }
        private Conv_Ovf_I_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_I_Un, operand, name) { }
        public Conv_Ovf_I_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U_ : CodeMatch {
        public Conv_Ovf_U_() : base(OpCodes.Conv_Ovf_U, null, null) { }
        private Conv_Ovf_U_(object operand, string name) : base(OpCodes.Conv_Ovf_U, operand, name) { }
        public Conv_Ovf_U_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U1_ : CodeMatch {
        public Conv_Ovf_U1_() : base(OpCodes.Conv_Ovf_U1, null, null) { }
        private Conv_Ovf_U1_(object operand, string name) : base(OpCodes.Conv_Ovf_U1, operand, name) { }
        public Conv_Ovf_U1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U1_Un_ : CodeMatch {
        public Conv_Ovf_U1_Un_() : base(OpCodes.Conv_Ovf_U1_Un, null, null) { }
        private Conv_Ovf_U1_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_U1_Un, operand, name) { }
        public Conv_Ovf_U1_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U2_ : CodeMatch {
        public Conv_Ovf_U2_() : base(OpCodes.Conv_Ovf_U2, null, null) { }
        private Conv_Ovf_U2_(object operand, string name) : base(OpCodes.Conv_Ovf_U2, operand, name) { }
        public Conv_Ovf_U2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U2_Un_ : CodeMatch {
        public Conv_Ovf_U2_Un_() : base(OpCodes.Conv_Ovf_U2_Un, null, null) { }
        private Conv_Ovf_U2_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_U2_Un, operand, name) { }
        public Conv_Ovf_U2_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U4_ : CodeMatch {
        public Conv_Ovf_U4_() : base(OpCodes.Conv_Ovf_U4, null, null) { }
        private Conv_Ovf_U4_(object operand, string name) : base(OpCodes.Conv_Ovf_U4, operand, name) { }
        public Conv_Ovf_U4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U4_Un_ : CodeMatch {
        public Conv_Ovf_U4_Un_() : base(OpCodes.Conv_Ovf_U4_Un, null, null) { }
        private Conv_Ovf_U4_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_U4_Un, operand, name) { }
        public Conv_Ovf_U4_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U8_ : CodeMatch {
        public Conv_Ovf_U8_() : base(OpCodes.Conv_Ovf_U8, null, null) { }
        private Conv_Ovf_U8_(object operand, string name) : base(OpCodes.Conv_Ovf_U8, operand, name) { }
        public Conv_Ovf_U8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U8_Un_ : CodeMatch {
        public Conv_Ovf_U8_Un_() : base(OpCodes.Conv_Ovf_U8_Un, null, null) { }
        private Conv_Ovf_U8_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_U8_Un, operand, name) { }
        public Conv_Ovf_U8_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_Ovf_U_Un_ : CodeMatch {
        public Conv_Ovf_U_Un_() : base(OpCodes.Conv_Ovf_U_Un, null, null) { }
        private Conv_Ovf_U_Un_(object operand, string name) : base(OpCodes.Conv_Ovf_U_Un, operand, name) { }
        public Conv_Ovf_U_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_R4_ : CodeMatch {
        public Conv_R4_() : base(OpCodes.Conv_R4, null, null) { }
        private Conv_R4_(object operand, string name) : base(OpCodes.Conv_R4, operand, name) { }
        public Conv_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_R8_ : CodeMatch {
        public Conv_R8_() : base(OpCodes.Conv_R8, null, null) { }
        private Conv_R8_(object operand, string name) : base(OpCodes.Conv_R8, operand, name) { }
        public Conv_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_R_Un_ : CodeMatch {
        public Conv_R_Un_() : base(OpCodes.Conv_R_Un, null, null) { }
        private Conv_R_Un_(object operand, string name) : base(OpCodes.Conv_R_Un, operand, name) { }
        public Conv_R_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_U_ : CodeMatch {
        public Conv_U_() : base(OpCodes.Conv_U, null, null) { }
        private Conv_U_(object operand, string name) : base(OpCodes.Conv_U, operand, name) { }
        public Conv_U_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_U1_ : CodeMatch {
        public Conv_U1_() : base(OpCodes.Conv_U1, null, null) { }
        private Conv_U1_(object operand, string name) : base(OpCodes.Conv_U1, operand, name) { }
        public Conv_U1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_U2_ : CodeMatch {
        public Conv_U2_() : base(OpCodes.Conv_U2, null, null) { }
        private Conv_U2_(object operand, string name) : base(OpCodes.Conv_U2, operand, name) { }
        public Conv_U2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_U4_ : CodeMatch {
        public Conv_U4_() : base(OpCodes.Conv_U4, null, null) { }
        private Conv_U4_(object operand, string name) : base(OpCodes.Conv_U4, operand, name) { }
        public Conv_U4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Conv_U8_ : CodeMatch {
        public Conv_U8_() : base(OpCodes.Conv_U8, null, null) { }
        private Conv_U8_(object operand, string name) : base(OpCodes.Conv_U8, operand, name) { }
        public Conv_U8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Cpblk_ : CodeMatch {
        public Cpblk_() : base(OpCodes.Cpblk, null, null) { }
        private Cpblk_(object operand, string name) : base(OpCodes.Cpblk, operand, name) { }
        public Cpblk_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Cpobj_ : CodeMatch {
        public Cpobj_() : base(OpCodes.Cpobj, null, null) { }
        private Cpobj_(object operand, string name) : base(OpCodes.Cpobj, operand, name) { }
        public Cpobj_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Div_ : CodeMatch {
        public Div_() : base(OpCodes.Div, null, null) { }
        private Div_(object operand, string name) : base(OpCodes.Div, operand, name) { }
        public Div_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Div_Un_ : CodeMatch {
        public Div_Un_() : base(OpCodes.Div_Un, null, null) { }
        private Div_Un_(object operand, string name) : base(OpCodes.Div_Un, operand, name) { }
        public Div_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Dup_ : CodeMatch {
        public Dup_() : base(OpCodes.Dup, null, null) { }
        private Dup_(object operand, string name) : base(OpCodes.Dup, operand, name) { }
        public Dup_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Endfilter_ : CodeMatch {
        public Endfilter_() : base(OpCodes.Endfilter, null, null) { }
        private Endfilter_(object operand, string name) : base(OpCodes.Endfilter, operand, name) { }
        public Endfilter_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Endfinally_ : CodeMatch {
        public Endfinally_() : base(OpCodes.Endfinally, null, null) { }
        private Endfinally_(object operand, string name) : base(OpCodes.Endfinally, operand, name) { }
        public Endfinally_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Initblk_ : CodeMatch {
        public Initblk_() : base(OpCodes.Initblk, null, null) { }
        private Initblk_(object operand, string name) : base(OpCodes.Initblk, operand, name) { }
        public Initblk_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Initobj_ : CodeMatch {
        public Initobj_() : base(OpCodes.Initobj, null, null) { }
        private Initobj_(object operand, string name) : base(OpCodes.Initobj, operand, name) { }
        public Initobj_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Isinst_ : CodeMatch {
        public Isinst_() : base(OpCodes.Isinst, null, null) { }
        private Isinst_(object operand, string name) : base(OpCodes.Isinst, operand, name) { }
        public Isinst_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Jmp_ : CodeMatch {
        public Jmp_() : base(OpCodes.Jmp, null, null) { }
        private Jmp_(object operand, string name) : base(OpCodes.Jmp, operand, name) { }
        public Jmp_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_ : CodeMatch {
        public Ldarg_() : base(OpCodes.Ldarg, null, null) { }
        private Ldarg_(object operand, string name) : base(OpCodes.Ldarg, operand, name) { }
        public Ldarg_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_0_ : CodeMatch {
        public Ldarg_0_() : base(OpCodes.Ldarg_0, null, null) { }
        private Ldarg_0_(object operand, string name) : base(OpCodes.Ldarg_0, operand, name) { }
        public Ldarg_0_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_1_ : CodeMatch {
        public Ldarg_1_() : base(OpCodes.Ldarg_1, null, null) { }
        private Ldarg_1_(object operand, string name) : base(OpCodes.Ldarg_1, operand, name) { }
        public Ldarg_1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_2_ : CodeMatch {
        public Ldarg_2_() : base(OpCodes.Ldarg_2, null, null) { }
        private Ldarg_2_(object operand, string name) : base(OpCodes.Ldarg_2, operand, name) { }
        public Ldarg_2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_3_ : CodeMatch {
        public Ldarg_3_() : base(OpCodes.Ldarg_3, null, null) { }
        private Ldarg_3_(object operand, string name) : base(OpCodes.Ldarg_3, operand, name) { }
        public Ldarg_3_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarg_S_ : CodeMatch {
        public Ldarg_S_() : base(OpCodes.Ldarg_S, null, null) { }
        private Ldarg_S_(object operand, string name) : base(OpCodes.Ldarg_S, operand, name) { }
        public Ldarg_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarga_ : CodeMatch {
        public Ldarga_() : base(OpCodes.Ldarga, null, null) { }
        private Ldarga_(object operand, string name) : base(OpCodes.Ldarga, operand, name) { }
        public Ldarga_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldarga_S_ : CodeMatch {
        public Ldarga_S_() : base(OpCodes.Ldarga_S, null, null) { }
        private Ldarga_S_(object operand, string name) : base(OpCodes.Ldarga_S, operand, name) { }
        public Ldarga_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_ : CodeMatch {
        public Ldc_I4_() : base(OpCodes.Ldc_I4, null, null) { }
        private Ldc_I4_(object operand, string name) : base(OpCodes.Ldc_I4, operand, name) { }
        public Ldc_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_0_ : CodeMatch {
        public Ldc_I4_0_() : base(OpCodes.Ldc_I4_0, null, null) { }
        private Ldc_I4_0_(object operand, string name) : base(OpCodes.Ldc_I4_0, operand, name) { }
        public Ldc_I4_0_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_1_ : CodeMatch {
        public Ldc_I4_1_() : base(OpCodes.Ldc_I4_1, null, null) { }
        private Ldc_I4_1_(object operand, string name) : base(OpCodes.Ldc_I4_1, operand, name) { }
        public Ldc_I4_1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_2_ : CodeMatch {
        public Ldc_I4_2_() : base(OpCodes.Ldc_I4_2, null, null) { }
        private Ldc_I4_2_(object operand, string name) : base(OpCodes.Ldc_I4_2, operand, name) { }
        public Ldc_I4_2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_3_ : CodeMatch {
        public Ldc_I4_3_() : base(OpCodes.Ldc_I4_3, null, null) { }
        private Ldc_I4_3_(object operand, string name) : base(OpCodes.Ldc_I4_3, operand, name) { }
        public Ldc_I4_3_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_4_ : CodeMatch {
        public Ldc_I4_4_() : base(OpCodes.Ldc_I4_4, null, null) { }
        private Ldc_I4_4_(object operand, string name) : base(OpCodes.Ldc_I4_4, operand, name) { }
        public Ldc_I4_4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_5_ : CodeMatch {
        public Ldc_I4_5_() : base(OpCodes.Ldc_I4_5, null, null) { }
        private Ldc_I4_5_(object operand, string name) : base(OpCodes.Ldc_I4_5, operand, name) { }
        public Ldc_I4_5_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_6_ : CodeMatch {
        public Ldc_I4_6_() : base(OpCodes.Ldc_I4_6, null, null) { }
        private Ldc_I4_6_(object operand, string name) : base(OpCodes.Ldc_I4_6, operand, name) { }
        public Ldc_I4_6_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_7_ : CodeMatch {
        public Ldc_I4_7_() : base(OpCodes.Ldc_I4_7, null, null) { }
        private Ldc_I4_7_(object operand, string name) : base(OpCodes.Ldc_I4_7, operand, name) { }
        public Ldc_I4_7_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_8_ : CodeMatch {
        public Ldc_I4_8_() : base(OpCodes.Ldc_I4_8, null, null) { }
        private Ldc_I4_8_(object operand, string name) : base(OpCodes.Ldc_I4_8, operand, name) { }
        public Ldc_I4_8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_M1_ : CodeMatch {
        public Ldc_I4_M1_() : base(OpCodes.Ldc_I4_M1, null, null) { }
        private Ldc_I4_M1_(object operand, string name) : base(OpCodes.Ldc_I4_M1, operand, name) { }
        public Ldc_I4_M1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I4_S_ : CodeMatch {
        public Ldc_I4_S_() : base(OpCodes.Ldc_I4_S, null, null) { }
        private Ldc_I4_S_(object operand, string name) : base(OpCodes.Ldc_I4_S, operand, name) { }
        public Ldc_I4_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_I8_ : CodeMatch {
        public Ldc_I8_() : base(OpCodes.Ldc_I8, null, null) { }
        private Ldc_I8_(object operand, string name) : base(OpCodes.Ldc_I8, operand, name) { }
        public Ldc_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_R4_ : CodeMatch {
        public Ldc_R4_() : base(OpCodes.Ldc_R4, null, null) { }
        private Ldc_R4_(object operand, string name) : base(OpCodes.Ldc_R4, operand, name) { }
        public Ldc_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldc_R8_ : CodeMatch {
        public Ldc_R8_() : base(OpCodes.Ldc_R8, null, null) { }
        private Ldc_R8_(object operand, string name) : base(OpCodes.Ldc_R8, operand, name) { }
        public Ldc_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_ : CodeMatch {
        public Ldelem_() : base(OpCodes.Ldelem, null, null) { }
        private Ldelem_(object operand, string name) : base(OpCodes.Ldelem, operand, name) { }
        public Ldelem_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_I_ : CodeMatch {
        public Ldelem_I_() : base(OpCodes.Ldelem_I, null, null) { }
        private Ldelem_I_(object operand, string name) : base(OpCodes.Ldelem_I, operand, name) { }
        public Ldelem_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_I1_ : CodeMatch {
        public Ldelem_I1_() : base(OpCodes.Ldelem_I1, null, null) { }
        private Ldelem_I1_(object operand, string name) : base(OpCodes.Ldelem_I1, operand, name) { }
        public Ldelem_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_I2_ : CodeMatch {
        public Ldelem_I2_() : base(OpCodes.Ldelem_I2, null, null) { }
        private Ldelem_I2_(object operand, string name) : base(OpCodes.Ldelem_I2, operand, name) { }
        public Ldelem_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_I4_ : CodeMatch {
        public Ldelem_I4_() : base(OpCodes.Ldelem_I4, null, null) { }
        private Ldelem_I4_(object operand, string name) : base(OpCodes.Ldelem_I4, operand, name) { }
        public Ldelem_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_I8_ : CodeMatch {
        public Ldelem_I8_() : base(OpCodes.Ldelem_I8, null, null) { }
        private Ldelem_I8_(object operand, string name) : base(OpCodes.Ldelem_I8, operand, name) { }
        public Ldelem_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_R4_ : CodeMatch {
        public Ldelem_R4_() : base(OpCodes.Ldelem_R4, null, null) { }
        private Ldelem_R4_(object operand, string name) : base(OpCodes.Ldelem_R4, operand, name) { }
        public Ldelem_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_R8_ : CodeMatch {
        public Ldelem_R8_() : base(OpCodes.Ldelem_R8, null, null) { }
        private Ldelem_R8_(object operand, string name) : base(OpCodes.Ldelem_R8, operand, name) { }
        public Ldelem_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_Ref_ : CodeMatch {
        public Ldelem_Ref_() : base(OpCodes.Ldelem_Ref, null, null) { }
        private Ldelem_Ref_(object operand, string name) : base(OpCodes.Ldelem_Ref, operand, name) { }
        public Ldelem_Ref_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_U1_ : CodeMatch {
        public Ldelem_U1_() : base(OpCodes.Ldelem_U1, null, null) { }
        private Ldelem_U1_(object operand, string name) : base(OpCodes.Ldelem_U1, operand, name) { }
        public Ldelem_U1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_U2_ : CodeMatch {
        public Ldelem_U2_() : base(OpCodes.Ldelem_U2, null, null) { }
        private Ldelem_U2_(object operand, string name) : base(OpCodes.Ldelem_U2, operand, name) { }
        public Ldelem_U2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelem_U4_ : CodeMatch {
        public Ldelem_U4_() : base(OpCodes.Ldelem_U4, null, null) { }
        private Ldelem_U4_(object operand, string name) : base(OpCodes.Ldelem_U4, operand, name) { }
        public Ldelem_U4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldelema_ : CodeMatch {
        public Ldelema_() : base(OpCodes.Ldelema, null, null) { }
        private Ldelema_(object operand, string name) : base(OpCodes.Ldelema, operand, name) { }
        public Ldelema_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldfld_ : CodeMatch {
        public Ldfld_() : base(OpCodes.Ldfld, null, null) { }
        private Ldfld_(object operand, string name) : base(OpCodes.Ldfld, operand, name) { }
        public Ldfld_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldflda_ : CodeMatch {
        public Ldflda_() : base(OpCodes.Ldflda, null, null) { }
        private Ldflda_(object operand, string name) : base(OpCodes.Ldflda, operand, name) { }
        public Ldflda_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldftn_ : CodeMatch {
        public Ldftn_() : base(OpCodes.Ldftn, null, null) { }
        private Ldftn_(object operand, string name) : base(OpCodes.Ldftn, operand, name) { }
        public Ldftn_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_I_ : CodeMatch {
        public Ldind_I_() : base(OpCodes.Ldind_I, null, null) { }
        private Ldind_I_(object operand, string name) : base(OpCodes.Ldind_I, operand, name) { }
        public Ldind_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_I1_ : CodeMatch {
        public Ldind_I1_() : base(OpCodes.Ldind_I1, null, null) { }
        private Ldind_I1_(object operand, string name) : base(OpCodes.Ldind_I1, operand, name) { }
        public Ldind_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_I2_ : CodeMatch {
        public Ldind_I2_() : base(OpCodes.Ldind_I2, null, null) { }
        private Ldind_I2_(object operand, string name) : base(OpCodes.Ldind_I2, operand, name) { }
        public Ldind_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_I4_ : CodeMatch {
        public Ldind_I4_() : base(OpCodes.Ldind_I4, null, null) { }
        private Ldind_I4_(object operand, string name) : base(OpCodes.Ldind_I4, operand, name) { }
        public Ldind_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_I8_ : CodeMatch {
        public Ldind_I8_() : base(OpCodes.Ldind_I8, null, null) { }
        private Ldind_I8_(object operand, string name) : base(OpCodes.Ldind_I8, operand, name) { }
        public Ldind_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_R4_ : CodeMatch {
        public Ldind_R4_() : base(OpCodes.Ldind_R4, null, null) { }
        private Ldind_R4_(object operand, string name) : base(OpCodes.Ldind_R4, operand, name) { }
        public Ldind_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_R8_ : CodeMatch {
        public Ldind_R8_() : base(OpCodes.Ldind_R8, null, null) { }
        private Ldind_R8_(object operand, string name) : base(OpCodes.Ldind_R8, operand, name) { }
        public Ldind_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_Ref_ : CodeMatch {
        public Ldind_Ref_() : base(OpCodes.Ldind_Ref, null, null) { }
        private Ldind_Ref_(object operand, string name) : base(OpCodes.Ldind_Ref, operand, name) { }
        public Ldind_Ref_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_U1_ : CodeMatch {
        public Ldind_U1_() : base(OpCodes.Ldind_U1, null, null) { }
        private Ldind_U1_(object operand, string name) : base(OpCodes.Ldind_U1, operand, name) { }
        public Ldind_U1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_U2_ : CodeMatch {
        public Ldind_U2_() : base(OpCodes.Ldind_U2, null, null) { }
        private Ldind_U2_(object operand, string name) : base(OpCodes.Ldind_U2, operand, name) { }
        public Ldind_U2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldind_U4_ : CodeMatch {
        public Ldind_U4_() : base(OpCodes.Ldind_U4, null, null) { }
        private Ldind_U4_(object operand, string name) : base(OpCodes.Ldind_U4, operand, name) { }
        public Ldind_U4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldlen_ : CodeMatch {
        public Ldlen_() : base(OpCodes.Ldlen, null, null) { }
        private Ldlen_(object operand, string name) : base(OpCodes.Ldlen, operand, name) { }
        public Ldlen_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_ : CodeMatch {
        public Ldloc_() : base(OpCodes.Ldloc, null, null) { }
        private Ldloc_(object operand, string name) : base(OpCodes.Ldloc, operand, name) { }
        public Ldloc_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_0_ : CodeMatch {
        public Ldloc_0_() : base(OpCodes.Ldloc_0, null, null) { }
        private Ldloc_0_(object operand, string name) : base(OpCodes.Ldloc_0, operand, name) { }
        public Ldloc_0_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_1_ : CodeMatch {
        public Ldloc_1_() : base(OpCodes.Ldloc_1, null, null) { }
        private Ldloc_1_(object operand, string name) : base(OpCodes.Ldloc_1, operand, name) { }
        public Ldloc_1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_2_ : CodeMatch {
        public Ldloc_2_() : base(OpCodes.Ldloc_2, null, null) { }
        private Ldloc_2_(object operand, string name) : base(OpCodes.Ldloc_2, operand, name) { }
        public Ldloc_2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_3_ : CodeMatch {
        public Ldloc_3_() : base(OpCodes.Ldloc_3, null, null) { }
        private Ldloc_3_(object operand, string name) : base(OpCodes.Ldloc_3, operand, name) { }
        public Ldloc_3_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloc_S_ : CodeMatch {
        public Ldloc_S_() : base(OpCodes.Ldloc_S, null, null) { }
        private Ldloc_S_(object operand, string name) : base(OpCodes.Ldloc_S, operand, name) { }
        public Ldloc_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloca_ : CodeMatch {
        public Ldloca_() : base(OpCodes.Ldloca, null, null) { }
        private Ldloca_(object operand, string name) : base(OpCodes.Ldloca, operand, name) { }
        public Ldloca_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldloca_S_ : CodeMatch {
        public Ldloca_S_() : base(OpCodes.Ldloca_S, null, null) { }
        private Ldloca_S_(object operand, string name) : base(OpCodes.Ldloca_S, operand, name) { }
        public Ldloca_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldnull_ : CodeMatch {
        public Ldnull_() : base(OpCodes.Ldnull, null, null) { }
        private Ldnull_(object operand, string name) : base(OpCodes.Ldnull, operand, name) { }
        public Ldnull_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldobj_ : CodeMatch {
        public Ldobj_() : base(OpCodes.Ldobj, null, null) { }
        private Ldobj_(object operand, string name) : base(OpCodes.Ldobj, operand, name) { }
        public Ldobj_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldsfld_ : CodeMatch {
        public Ldsfld_() : base(OpCodes.Ldsfld, null, null) { }
        private Ldsfld_(object operand, string name) : base(OpCodes.Ldsfld, operand, name) { }
        public Ldsfld_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldsflda_ : CodeMatch {
        public Ldsflda_() : base(OpCodes.Ldsflda, null, null) { }
        private Ldsflda_(object operand, string name) : base(OpCodes.Ldsflda, operand, name) { }
        public Ldsflda_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldstr_ : CodeMatch {
        public Ldstr_() : base(OpCodes.Ldstr, null, null) { }
        private Ldstr_(object operand, string name) : base(OpCodes.Ldstr, operand, name) { }
        public Ldstr_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldtoken_ : CodeMatch {
        public Ldtoken_() : base(OpCodes.Ldtoken, null, null) { }
        private Ldtoken_(object operand, string name) : base(OpCodes.Ldtoken, operand, name) { }
        public Ldtoken_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ldvirtftn_ : CodeMatch {
        public Ldvirtftn_() : base(OpCodes.Ldvirtftn, null, null) { }
        private Ldvirtftn_(object operand, string name) : base(OpCodes.Ldvirtftn, operand, name) { }
        public Ldvirtftn_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Leave_ : CodeMatch {
        public Leave_() : base(OpCodes.Leave, null, null) { }
        private Leave_(object operand, string name) : base(OpCodes.Leave, operand, name) { }
        public Leave_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Leave_S_ : CodeMatch {
        public Leave_S_() : base(OpCodes.Leave_S, null, null) { }
        private Leave_S_(object operand, string name) : base(OpCodes.Leave_S, operand, name) { }
        public Leave_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Localloc_ : CodeMatch {
        public Localloc_() : base(OpCodes.Localloc, null, null) { }
        private Localloc_(object operand, string name) : base(OpCodes.Localloc, operand, name) { }
        public Localloc_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Mkrefany_ : CodeMatch {
        public Mkrefany_() : base(OpCodes.Mkrefany, null, null) { }
        private Mkrefany_(object operand, string name) : base(OpCodes.Mkrefany, operand, name) { }
        public Mkrefany_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Mul_ : CodeMatch {
        public Mul_() : base(OpCodes.Mul, null, null) { }
        private Mul_(object operand, string name) : base(OpCodes.Mul, operand, name) { }
        public Mul_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Mul_Ovf_ : CodeMatch {
        public Mul_Ovf_() : base(OpCodes.Mul_Ovf, null, null) { }
        private Mul_Ovf_(object operand, string name) : base(OpCodes.Mul_Ovf, operand, name) { }
        public Mul_Ovf_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Mul_Ovf_Un_ : CodeMatch {
        public Mul_Ovf_Un_() : base(OpCodes.Mul_Ovf_Un, null, null) { }
        private Mul_Ovf_Un_(object operand, string name) : base(OpCodes.Mul_Ovf_Un, operand, name) { }
        public Mul_Ovf_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Neg_ : CodeMatch {
        public Neg_() : base(OpCodes.Neg, null, null) { }
        private Neg_(object operand, string name) : base(OpCodes.Neg, operand, name) { }
        public Neg_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Newarr_ : CodeMatch {
        public Newarr_() : base(OpCodes.Newarr, null, null) { }
        private Newarr_(object operand, string name) : base(OpCodes.Newarr, operand, name) { }
        public Newarr_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Newobj_ : CodeMatch {
        public Newobj_() : base(OpCodes.Newobj, null, null) { }
        private Newobj_(object operand, string name) : base(OpCodes.Newobj, operand, name) { }
        public Newobj_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Nop_ : CodeMatch {
        public Nop_() : base(OpCodes.Nop, null, null) { }
        private Nop_(object operand, string name) : base(OpCodes.Nop, operand, name) { }
        public Nop_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Not_ : CodeMatch {
        public Not_() : base(OpCodes.Not, null, null) { }
        private Not_(object operand, string name) : base(OpCodes.Not, operand, name) { }
        public Not_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Or_ : CodeMatch {
        public Or_() : base(OpCodes.Or, null, null) { }
        private Or_(object operand, string name) : base(OpCodes.Or, operand, name) { }
        public Or_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Pop_ : CodeMatch {
        public Pop_() : base(OpCodes.Pop, null, null) { }
        private Pop_(object operand, string name) : base(OpCodes.Pop, operand, name) { }
        public Pop_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix1_ : CodeMatch {
        public Prefix1_() : base(OpCodes.Prefix1, null, null) { }
        private Prefix1_(object operand, string name) : base(OpCodes.Prefix1, operand, name) { }
        public Prefix1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix2_ : CodeMatch {
        public Prefix2_() : base(OpCodes.Prefix2, null, null) { }
        private Prefix2_(object operand, string name) : base(OpCodes.Prefix2, operand, name) { }
        public Prefix2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix3_ : CodeMatch {
        public Prefix3_() : base(OpCodes.Prefix3, null, null) { }
        private Prefix3_(object operand, string name) : base(OpCodes.Prefix3, operand, name) { }
        public Prefix3_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix4_ : CodeMatch {
        public Prefix4_() : base(OpCodes.Prefix4, null, null) { }
        private Prefix4_(object operand, string name) : base(OpCodes.Prefix4, operand, name) { }
        public Prefix4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix5_ : CodeMatch {
        public Prefix5_() : base(OpCodes.Prefix5, null, null) { }
        private Prefix5_(object operand, string name) : base(OpCodes.Prefix5, operand, name) { }
        public Prefix5_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix6_ : CodeMatch {
        public Prefix6_() : base(OpCodes.Prefix6, null, null) { }
        private Prefix6_(object operand, string name) : base(OpCodes.Prefix6, operand, name) { }
        public Prefix6_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefix7_ : CodeMatch {
        public Prefix7_() : base(OpCodes.Prefix7, null, null) { }
        private Prefix7_(object operand, string name) : base(OpCodes.Prefix7, operand, name) { }
        public Prefix7_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Prefixref_ : CodeMatch {
        public Prefixref_() : base(OpCodes.Prefixref, null, null) { }
        private Prefixref_(object operand, string name) : base(OpCodes.Prefixref, operand, name) { }
        public Prefixref_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Readonly_ : CodeMatch {
        public Readonly_() : base(OpCodes.Readonly, null, null) { }
        private Readonly_(object operand, string name) : base(OpCodes.Readonly, operand, name) { }
        public Readonly_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Refanytype_ : CodeMatch {
        public Refanytype_() : base(OpCodes.Refanytype, null, null) { }
        private Refanytype_(object operand, string name) : base(OpCodes.Refanytype, operand, name) { }
        public Refanytype_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Refanyval_ : CodeMatch {
        public Refanyval_() : base(OpCodes.Refanyval, null, null) { }
        private Refanyval_(object operand, string name) : base(OpCodes.Refanyval, operand, name) { }
        public Refanyval_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Rem_ : CodeMatch {
        public Rem_() : base(OpCodes.Rem, null, null) { }
        private Rem_(object operand, string name) : base(OpCodes.Rem, operand, name) { }
        public Rem_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Rem_Un_ : CodeMatch {
        public Rem_Un_() : base(OpCodes.Rem_Un, null, null) { }
        private Rem_Un_(object operand, string name) : base(OpCodes.Rem_Un, operand, name) { }
        public Rem_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Ret_ : CodeMatch {
        public Ret_() : base(OpCodes.Ret, null, null) { }
        private Ret_(object operand, string name) : base(OpCodes.Ret, operand, name) { }
        public Ret_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Rethrow_ : CodeMatch {
        public Rethrow_() : base(OpCodes.Rethrow, null, null) { }
        private Rethrow_(object operand, string name) : base(OpCodes.Rethrow, operand, name) { }
        public Rethrow_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Shl_ : CodeMatch {
        public Shl_() : base(OpCodes.Shl, null, null) { }
        private Shl_(object operand, string name) : base(OpCodes.Shl, operand, name) { }
        public Shl_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Shr_ : CodeMatch {
        public Shr_() : base(OpCodes.Shr, null, null) { }
        private Shr_(object operand, string name) : base(OpCodes.Shr, operand, name) { }
        public Shr_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Shr_Un_ : CodeMatch {
        public Shr_Un_() : base(OpCodes.Shr_Un, null, null) { }
        private Shr_Un_(object operand, string name) : base(OpCodes.Shr_Un, operand, name) { }
        public Shr_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Sizeof_ : CodeMatch {
        public Sizeof_() : base(OpCodes.Sizeof, null, null) { }
        private Sizeof_(object operand, string name) : base(OpCodes.Sizeof, operand, name) { }
        public Sizeof_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Starg_ : CodeMatch {
        public Starg_() : base(OpCodes.Starg, null, null) { }
        private Starg_(object operand, string name) : base(OpCodes.Starg, operand, name) { }
        public Starg_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Starg_S_ : CodeMatch {
        public Starg_S_() : base(OpCodes.Starg_S, null, null) { }
        private Starg_S_(object operand, string name) : base(OpCodes.Starg_S, operand, name) { }
        public Starg_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_ : CodeMatch {
        public Stelem_() : base(OpCodes.Stelem, null, null) { }
        private Stelem_(object operand, string name) : base(OpCodes.Stelem, operand, name) { }
        public Stelem_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_I_ : CodeMatch {
        public Stelem_I_() : base(OpCodes.Stelem_I, null, null) { }
        private Stelem_I_(object operand, string name) : base(OpCodes.Stelem_I, operand, name) { }
        public Stelem_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_I1_ : CodeMatch {
        public Stelem_I1_() : base(OpCodes.Stelem_I1, null, null) { }
        private Stelem_I1_(object operand, string name) : base(OpCodes.Stelem_I1, operand, name) { }
        public Stelem_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_I2_ : CodeMatch {
        public Stelem_I2_() : base(OpCodes.Stelem_I2, null, null) { }
        private Stelem_I2_(object operand, string name) : base(OpCodes.Stelem_I2, operand, name) { }
        public Stelem_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_I4_ : CodeMatch {
        public Stelem_I4_() : base(OpCodes.Stelem_I4, null, null) { }
        private Stelem_I4_(object operand, string name) : base(OpCodes.Stelem_I4, operand, name) { }
        public Stelem_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_I8_ : CodeMatch {
        public Stelem_I8_() : base(OpCodes.Stelem_I8, null, null) { }
        private Stelem_I8_(object operand, string name) : base(OpCodes.Stelem_I8, operand, name) { }
        public Stelem_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_R4_ : CodeMatch {
        public Stelem_R4_() : base(OpCodes.Stelem_R4, null, null) { }
        private Stelem_R4_(object operand, string name) : base(OpCodes.Stelem_R4, operand, name) { }
        public Stelem_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_R8_ : CodeMatch {
        public Stelem_R8_() : base(OpCodes.Stelem_R8, null, null) { }
        private Stelem_R8_(object operand, string name) : base(OpCodes.Stelem_R8, operand, name) { }
        public Stelem_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stelem_Ref_ : CodeMatch {
        public Stelem_Ref_() : base(OpCodes.Stelem_Ref, null, null) { }
        private Stelem_Ref_(object operand, string name) : base(OpCodes.Stelem_Ref, operand, name) { }
        public Stelem_Ref_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stfld_ : CodeMatch {
        public Stfld_() : base(OpCodes.Stfld, null, null) { }
        private Stfld_(object operand, string name) : base(OpCodes.Stfld, operand, name) { }
        public Stfld_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_I_ : CodeMatch {
        public Stind_I_() : base(OpCodes.Stind_I, null, null) { }
        private Stind_I_(object operand, string name) : base(OpCodes.Stind_I, operand, name) { }
        public Stind_I_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_I1_ : CodeMatch {
        public Stind_I1_() : base(OpCodes.Stind_I1, null, null) { }
        private Stind_I1_(object operand, string name) : base(OpCodes.Stind_I1, operand, name) { }
        public Stind_I1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_I2_ : CodeMatch {
        public Stind_I2_() : base(OpCodes.Stind_I2, null, null) { }
        private Stind_I2_(object operand, string name) : base(OpCodes.Stind_I2, operand, name) { }
        public Stind_I2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_I4_ : CodeMatch {
        public Stind_I4_() : base(OpCodes.Stind_I4, null, null) { }
        private Stind_I4_(object operand, string name) : base(OpCodes.Stind_I4, operand, name) { }
        public Stind_I4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_I8_ : CodeMatch {
        public Stind_I8_() : base(OpCodes.Stind_I8, null, null) { }
        private Stind_I8_(object operand, string name) : base(OpCodes.Stind_I8, operand, name) { }
        public Stind_I8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_R4_ : CodeMatch {
        public Stind_R4_() : base(OpCodes.Stind_R4, null, null) { }
        private Stind_R4_(object operand, string name) : base(OpCodes.Stind_R4, operand, name) { }
        public Stind_R4_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_R8_ : CodeMatch {
        public Stind_R8_() : base(OpCodes.Stind_R8, null, null) { }
        private Stind_R8_(object operand, string name) : base(OpCodes.Stind_R8, operand, name) { }
        public Stind_R8_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stind_Ref_ : CodeMatch {
        public Stind_Ref_() : base(OpCodes.Stind_Ref, null, null) { }
        private Stind_Ref_(object operand, string name) : base(OpCodes.Stind_Ref, operand, name) { }
        public Stind_Ref_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_ : CodeMatch {
        public Stloc_() : base(OpCodes.Stloc, null, null) { }
        private Stloc_(object operand, string name) : base(OpCodes.Stloc, operand, name) { }
        public Stloc_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_0_ : CodeMatch {
        public Stloc_0_() : base(OpCodes.Stloc_0, null, null) { }
        private Stloc_0_(object operand, string name) : base(OpCodes.Stloc_0, operand, name) { }
        public Stloc_0_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_1_ : CodeMatch {
        public Stloc_1_() : base(OpCodes.Stloc_1, null, null) { }
        private Stloc_1_(object operand, string name) : base(OpCodes.Stloc_1, operand, name) { }
        public Stloc_1_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_2_ : CodeMatch {
        public Stloc_2_() : base(OpCodes.Stloc_2, null, null) { }
        private Stloc_2_(object operand, string name) : base(OpCodes.Stloc_2, operand, name) { }
        public Stloc_2_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_3_ : CodeMatch {
        public Stloc_3_() : base(OpCodes.Stloc_3, null, null) { }
        private Stloc_3_(object operand, string name) : base(OpCodes.Stloc_3, operand, name) { }
        public Stloc_3_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stloc_S_ : CodeMatch {
        public Stloc_S_() : base(OpCodes.Stloc_S, null, null) { }
        private Stloc_S_(object operand, string name) : base(OpCodes.Stloc_S, operand, name) { }
        public Stloc_S_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stobj_ : CodeMatch {
        public Stobj_() : base(OpCodes.Stobj, null, null) { }
        private Stobj_(object operand, string name) : base(OpCodes.Stobj, operand, name) { }
        public Stobj_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Stsfld_ : CodeMatch {
        public Stsfld_() : base(OpCodes.Stsfld, null, null) { }
        private Stsfld_(object operand, string name) : base(OpCodes.Stsfld, operand, name) { }
        public Stsfld_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Sub_ : CodeMatch {
        public Sub_() : base(OpCodes.Sub, null, null) { }
        private Sub_(object operand, string name) : base(OpCodes.Sub, operand, name) { }
        public Sub_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Sub_Ovf_ : CodeMatch {
        public Sub_Ovf_() : base(OpCodes.Sub_Ovf, null, null) { }
        private Sub_Ovf_(object operand, string name) : base(OpCodes.Sub_Ovf, operand, name) { }
        public Sub_Ovf_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Sub_Ovf_Un_ : CodeMatch {
        public Sub_Ovf_Un_() : base(OpCodes.Sub_Ovf_Un, null, null) { }
        private Sub_Ovf_Un_(object operand, string name) : base(OpCodes.Sub_Ovf_Un, operand, name) { }
        public Sub_Ovf_Un_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Switch_ : CodeMatch {
        public Switch_() : base(OpCodes.Switch, null, null) { }
        private Switch_(object operand, string name) : base(OpCodes.Switch, operand, name) { }
        public Switch_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Tailcall_ : CodeMatch {
        public Tailcall_() : base(OpCodes.Tailcall, null, null) { }
        private Tailcall_(object operand, string name) : base(OpCodes.Tailcall, operand, name) { }
        public Tailcall_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Throw_ : CodeMatch {
        public Throw_() : base(OpCodes.Throw, null, null) { }
        private Throw_(object operand, string name) : base(OpCodes.Throw, operand, name) { }
        public Throw_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Unaligned_ : CodeMatch {
        public Unaligned_() : base(OpCodes.Unaligned, null, null) { }
        private Unaligned_(object operand, string name) : base(OpCodes.Unaligned, operand, name) { }
        public Unaligned_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Unbox_ : CodeMatch {
        public Unbox_() : base(OpCodes.Unbox, null, null) { }
        private Unbox_(object operand, string name) : base(OpCodes.Unbox, operand, name) { }
        public Unbox_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Unbox_Any_ : CodeMatch {
        public Unbox_Any_() : base(OpCodes.Unbox_Any, null, null) { }
        private Unbox_Any_(object operand, string name) : base(OpCodes.Unbox_Any, operand, name) { }
        public Unbox_Any_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Volatile_ : CodeMatch {
        public Volatile_() : base(OpCodes.Volatile, null, null) { }
        private Volatile_(object operand, string name) : base(OpCodes.Volatile, operand, name) { }
        public Volatile_ this[object operand = null, string name = null] => new(operand, name);
    }
    public class Xor_ : CodeMatch {
        public Xor_() : base(OpCodes.Xor, null, null) { }
        private Xor_(object operand, string name) : base(OpCodes.Xor, operand, name) { }
        public Xor_ this[object operand = null, string name = null] => new(operand, name);
    }

    public class Operand_ : CodeMatch {
        public Operand_() : base(null, null, null) { }
        private Operand_(object operand, string name) : base(null, operand, name) { }
        public Operand_ this[object operand = null, string name = null] => new(operand, name);
    }
}
