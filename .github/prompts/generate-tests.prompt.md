---
description: "Generate comprehensive unit tests and integration tests for a given class or component"
agent: "test-writer"
argument-hint: "Class name to test (e.g., ProductController)"
---

Generate comprehensive tests for the specified class:

1. Read the source code and identify all testable methods
2. Create **unit tests** with Moq for all dependencies
3. Create **integration tests** if it's a controller
4. Cover: happy path, error cases, edge cases, boundary values
5. Follow naming: `MethodName_Scenario_ExpectedResult`
6. Use FluentAssertions for all assertions
7. Run the tests to verify they pass
