#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <math.h>
struct Rectangle {
    long left, top, right, bottom;
};

struct Point {
    long x, y;
};

typedef double integrate_cb(double);

extern "C" __declspec(dllexport) double integrate(double start, double end, integrate_cb *cb, long steps) { 
    // TODO: Implement me
    return .333333333333;
}

extern "C" __declspec(dllexport) char *GetString(void) { 
    return "foo"; 
}

extern "C" __declspec(dllexport) int PointInRect(Rectangle *x, Point pt) { 
    return pt.y >= x->top && pt.y <= x->bottom && pt.x >= x->left && pt.x <= x->right ? 1 : 0;
}

extern "C" __declspec(dllexport) double _testfunc_D_bhilfD(char b, short h, int i, long l, float f, double d) { 
    return b + h + i + l + f + d;
}

extern "C" __declspec(dllexport) int* _testfunc_ai8(int inp[8]) { 
    return 0;
}

extern "C" __declspec(dllexport) int _testfunc_byval(Point in, Point *out) { 
    out->x = in.x;
    out->y = in.y;
    return in.x + in.y;
}

extern "C" __declspec(dllexport) char* _testfunc_c_p_p(int *cnt, char **p) { 
    return p[(*cnt)-1];
}

typedef int intfunc(int);

extern "C" __declspec(dllexport) int _testfunc_callback_i_if(int i, intfunc *func) { 
    int res = 0;
    while (i != 0) {
        res += func(i);
        i /= 2;
    }
    return res;
}

typedef __int64 int64func(__int64);

extern "C" __declspec(dllexport) __int64 _testfunc_callback_q_qf(__int64 x, int64func *func) { 
    __int64 res = 0;
    while (x != 0) {
        res += func(x);
        x /= 2;
    }
    return res;
}

typedef int intPtrFunc(const int*);
extern "C" __declspec(dllexport) void _testfunc_callback_with_pointer(intPtrFunc* func) { 
    const int x[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
    func(x);
}

extern "C" __declspec(dllexport) double _testfunc_d_bhilfd(char b, short h, int i, long l, float f, double d) { 
    return b + h + i + l + f + d;
}

extern "C" __declspec(dllexport) float _testfunc_f_bhilfd(char b, short h, int i, long l, float f, double d) { 
    return b + h + i + l + f + d;
}
extern "C" __declspec(dllexport) int _testfunc_i_bhilfd(char b, short h, int i, long l, float f, double d) { 
    return b + h + i + l + f + d;
}
extern "C" __declspec(dllexport) void* _testfunc_p_p(void *p) { 
    return p;
}

extern "C" __declspec(dllexport) __int64 _testfunc_q_bhilfd(char b, short h, int i, long l, float f, double d) { 
    return b + h + i + l + f + d;
}
extern "C" __declspec(dllexport) __int64 _testfunc_q_bhilfdq(char b, short h, int i, long l, float f, double d, __int64 q) { 
    return b + h + i + l + f + d + q;
}

extern "C" __declspec(dllexport) void _testfunc_v(int x, int y, int *z) { 
    *z = x + y;
}

extern "C" __declspec(dllexport) int an_integer = 42;
extern "C" __declspec(dllexport) int get_an_integer(void) { 
    return an_integer;
}

typedef const char* strchrfunc(const char*, int);

extern "C" __declspec(dllexport) strchrfunc* get_strchr(void) { 
    return strchr;
}

extern "C" __declspec(dllexport) void my_free(void *p) { free(p); }

typedef int comparer(const void*, const void *);
extern "C" __declspec(dllexport) void my_qsort(void* base, size_t num, size_t size, comparer* cmp) { 
    qsort(base, num, size, cmp);
}
extern "C" __declspec(dllexport) double my_sqrt(double x) { 
    return sqrt(x);
}

extern "C" __declspec(dllexport) char * my_strchr(char *inp, char p) { 
    return strchr(inp, p);
}
extern "C" __declspec(dllexport) char* my_strdup(char *inp) { 
    return strdup(inp);
}
extern "C" __declspec(dllexport) char* my_strtok(char *str, const char *delims) { 
    return strtok(str, delims);
}
extern "C" __declspec(dllexport) wchar_t* my_wcsdup(wchar_t* str) { 
    return wcsdup(str);
}
extern "C" __declspec(dllexport) size_t my_wcslen(wchar_t* str) { 
    return wcslen(str);
}

struct S2H {
    short x, y;
};
extern "C" __declspec(dllexport) S2H ret_2h_func(S2H x) { 
    S2H res;
    res.x = x.x * 2;
    res.y = x.y * 3;
    return res;
}
struct S8I {
    int a, b, c, d, e, f, g, h;
};
extern "C" __declspec(dllexport) S8I ret_8i_func(S8I x) { 
    S8I res;
    res.a = x.a * 2;
    res.b = x.b * 3;
    res.c = x.c * 4;
    res.d = x.d * 5;
    res.e = x.e * 6;
    res.f = x.f * 7;
    res.g = x.g * 8;
    res.h = x.h * 9;
    return res;
}

extern "C" __declspec(dllexport) S2H __stdcall s_ret_2h_func(S2H x) { 
    return ret_2h_func(x);
}
extern "C" __declspec(dllexport) S8I __stdcall s_ret_8i_func(S8I x) { 
    return ret_8i_func(x);
}

extern "C" __declspec(dllexport) __int64 last_tf_arg_s = 0;
extern "C" __declspec(dllexport) unsigned __int64 last_tf_arg_u = 0;


extern "C" __declspec(dllexport) unsigned char tf_B(unsigned char i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) long double tf_D(long double i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned short tf_H(unsigned short i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned int tf_I(unsigned int i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned long tf_L(unsigned long i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned __int64 tf_Q(unsigned __int64 i) { 
    last_tf_arg_u = i;
    return i / 3;
}


extern "C" __declspec(dllexport) signed char tf_b(signed char i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) double tf_d(double i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) float tf_f(float i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) short tf_h(short i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) int tf_i(int i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) long tf_l(long i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) __int64 tf_q(__int64 i) { 
    last_tf_arg_s = i;
    return i / 3;
}

extern "C" __declspec(dllexport) void tv_i(int i) { 
    last_tf_arg_s = i;    
}

extern "C" __declspec(dllexport) unsigned char tf_bB(char x, unsigned char y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) long double tf_bD(char x, long double y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned short tf_bH(char x, unsigned short y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned int tf_bI(char x, unsigned int y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned long tf_bL(char x, unsigned long y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned __int64 tf_bQ(char x, unsigned __int64 y) { 
    last_tf_arg_u = y;
    return y / 3;
}

extern "C" __declspec(dllexport) char tf_bb(char x, char y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) double tf_bd(char x, double y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) float tf_bf(char x, float y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) short tf_bh(char x, short y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) int tf_bi(char x, int y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) long tf_bl(char x, long y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) __int64 tf_bq(char x, __int64 y) { 
    last_tf_arg_s = y;
    return y / 3;
}


// stdcall versions

extern "C" __declspec(dllexport) unsigned char __stdcall s_tf_B(unsigned char i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) double __stdcall s_tf_D(double i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned short __stdcall s_tf_H(unsigned short i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned int __stdcall s_tf_I(unsigned int i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned long __stdcall s_tf_L(unsigned long i) { 
    last_tf_arg_u = i;
    return i / 3;
}
extern "C" __declspec(dllexport) unsigned __int64 __stdcall s_tf_Q(unsigned __int64 i) { 
    last_tf_arg_u = i;
    return i / 3;
}


extern "C" __declspec(dllexport) signed char __stdcall s_tf_b(signed char i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) double __stdcall s_tf_d(double i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) float __stdcall s_tf_f(float i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) short __stdcall s_tf_h(short i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) int __stdcall s_tf_i(int i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) long __stdcall s_tf_l(long i) { 
    last_tf_arg_s = i;
    return i / 3;
}
extern "C" __declspec(dllexport) __int64 __stdcall s_tf_q(__int64 i) { 
    last_tf_arg_s = i;
    return i / 3;
}

extern "C" __declspec(dllexport) void __stdcall s_tv_i(int i) { 
    last_tf_arg_s = i;    
}

extern "C" __declspec(dllexport) unsigned char __stdcall s_tf_bB(char x, unsigned char y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) double __stdcall s_tf_bD(char x, double y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned short __stdcall s_tf_bH(char x, unsigned short y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned int __stdcall s_tf_bI(char x, unsigned int y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned long __stdcall s_tf_bL(char x, unsigned long y) { 
    last_tf_arg_u = y;
    return y / 3;
}
extern "C" __declspec(dllexport) unsigned __int64 __stdcall s_tf_bQ(char x, unsigned __int64 y) { 
    last_tf_arg_u = y;
    return y / 3;
}

extern "C" __declspec(dllexport) char __stdcall s_tf_bb(char x, char y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) double __stdcall s_tf_bd(char x, double y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) float __stdcall s_tf_bf(char x, float y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) short __stdcall s_tf_bh(char x, short y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) int __stdcall s_tf_bi(char x, int y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) long __stdcall s_tf_bl(char x, long y) { 
    last_tf_arg_s = y;
    return y / 3;
}
extern "C" __declspec(dllexport) __int64 __stdcall s_tf_bq(char x, __int64 y) { 
    last_tf_arg_s = y;
    return y / 3;
}

struct Bits {
    int A : 1;
    int B : 2;
    int C : 3;
    int D : 4;
    int E : 5;
    int F : 6;
    int G : 7;
    int H : 8;
    int I : 9;
    
    short M : 1;
    short N : 2;
    short O : 3;
    short P : 4;
    short Q : 5;
    short R : 6;
    short S : 7;
};

extern "C" __declspec(dllexport) int unpack_bitfields(Bits *b, char c) { 
    switch(c) {
        case 'A': return b->A;
        case 'B': return b->B;
        case 'C': return b->C;
        case 'D': return b->D;
        case 'E': return b->E;
        case 'F': return b->F;
        case 'G': return b->G;
        case 'H': return b->H;
        case 'I': return b->I;
        
        case 'M': return b->M;
        case 'N': return b->N;
        case 'O': return b->O;
        case 'P': return b->P;
        case 'Q': return b->Q;
        case 'R': return b->R;
        case 'S': return b->S;
    }
    return 0x7fffffff;
}