#include <stdio.h>

int main(void)
{
    int a = 0;
    int b = 0;
    int result = 0;

    /* node: a */
    a = 10;
    /* node: b */
    b = 20;
    /* node: cond */
    if (a > b)
    {
        /* node: trueAssign */
        result = a;
    }
    else
    {
        /* node: falseAssign */
        result = b;
    }
    /* node: print */
    printf("%d\n", result);
    return 0;
}
