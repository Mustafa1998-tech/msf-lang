# Examples

This page collects practical MSF Lang examples you can run today.

## 1. Hello World

**File:** [`examples/hello.msf`](../../examples/hello.msf)

```msf
print("Hello, MSF!");
```

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

## 2. Variables and Reassignment

```msf
let name = "Mustafa";
var visits = 1;
visits = visits + 1;

print("Hello,", name);
print("Visits:", visits);
```

## 3. Conditionals and Functions

```msf
fn classify(n) {
    if (n < 0) {
        return "negative";
    } else if (n == 0) {
        return "zero";
    } else {
        return "positive";
    }
}

print(classify(-4), classify(0), classify(9));
```

## 4. Recursion

**File:** [`examples/factorial.msf`](../../examples/factorial.msf)

```msf
fn factorial(n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

print(factorial(10));
```

## 5. Logic and Short-Circuiting

```msf
print(false and (1 / 0 == 0));
print(true or (1 / 0 == 0));
print(not false);
```

The right-hand side is not evaluated when short-circuiting applies.

## 6. Common Errors (learning examples)

### Immutable assignment
```msf
let x = 10;
x = 20; // E3005
```

### Type mismatch
```msf
print(1 + "2"); // E4004
```

### Division by zero
```msf
print(10 / 0); // E4002
```

## Next

- Read [Syntax & Language Basics](syntax.md) for the full v0.1 language surface
- Read [Diagnostics & Errors](diagnostics.md) to understand error messages
- Check the [specification](../en/SPEC-v0.1.md) for normative behavior and edge cases
