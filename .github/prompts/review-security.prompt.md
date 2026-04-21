---
description: "Review code for security vulnerabilities following OWASP Top 10"
agent: "security-reviewer"
---

Perform a security audit of the current codebase or specified component:

1. Check for **OWASP Top 10** vulnerabilities
2. Review **JWT authentication** configuration
3. Validate **input sanitization** via FluentValidation
4. Check for **hardcoded secrets** in code and configuration
5. Review **rate limiting** configuration
6. Verify **security headers** (HSTS, CSP, X-Content-Type-Options)
7. Check for **SQL injection** risks in Dapper queries

Return findings organized by severity (Critical → Low) with specific remediation steps.
