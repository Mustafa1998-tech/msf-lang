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

## 7. Practical Lists and Loops (v0.2 draft)

**File:** [`examples/practical-lists.msf`](../../examples/practical-lists.msf)

```msf
var numbers = [1, 2, 3, 4, 5];
var doubled = [];
var i = 0;

while (i < len(numbers)) {
    append(doubled, numbers[i] * 2);
    i = i + 1;
}

var total = 0;
i = 0;
while (i < len(doubled)) {
    total = total + doubled[i];
    i = i + 1;
}

print("doubled:", doubled);
print("total:", total);
```

Output:
```text
doubled: [2, 4, 6, 8, 10]
total: 30
```

## 8. Local Modules (v0.3)

**Files:** [`examples/modules/math.msf`](../../examples/modules/math.msf) + [`examples/modules/main.msf`](../../examples/modules/main.msf)

```msf
// math.msf
export fn square(n) { return n * n; }
export var PI = 3.5;
var secret = 42; // module-private

export fn circleArea(radius) {
    return PI * square(radius);
}

// main.msf (run from the examples/modules directory)
import "./math.msf";

print(math.square(9));     // 81
print(math.circleArea(2)); // 14
```

Run:

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/modules/main.msf
```

Output:

```text
81
3.5
14
```

Rules worth knowing:

- Only `export`ed names are visible; access is always qualified (`math.square(9)`).
- A module executes once per process, in dependency order.
- Circular imports and unexported access are hard errors (`E4014`, `E4013`); unqualified use is `E3001`.

See the [v0.3 modules spec](../en/SPEC-v0.3-modules.md) for the complete rules.

## Next

- Read [Syntax & Language Basics](syntax.md) for the full v0.1 language surface
- Read [Diagnostics & Errors](diagnostics.md) to understand error messages
- Check the [specifications](../en/SPEC-v0.1.md) and [v0.3 modules spec](../en/SPEC-v0.3-modules.md) for normative behavior and edge cases
