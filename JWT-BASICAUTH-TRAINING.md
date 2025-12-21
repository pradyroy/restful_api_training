# JWT & Basic Authentication — Practical Training Guide  
*(Using .NET Minimal APIs + Postman)*

This document is a **hands-on training guide** to understand and demonstrate:

- What **JWT (JSON Web Token)** is
- How JWT works internally (structure, Base64, signing, validation)
- How **Basic Authentication** works
- How both are used in the **same application**, with the **same database and users**
- A **live Postman demo flow** with 4 API calls

This guide is intentionally **practical, implementation-aligned, and security-correct**.

---

## 1. Why Authentication Exists in APIs

Authentication answers one core question:

> **“Who is calling this API?”**

Modern APIs must answer this **without server-side sessions**, because:
- APIs are stateless
- APIs scale horizontally
- REST discourages server memory tied to clients

JWT and Basic Auth are two ways of solving this.

---

## 2. What is JWT (JSON Web Token)?

A **JWT** is a **self-contained, stateless authentication token**.

Key properties:
- Issued **after successful login**
- Sent by the client on **every protected request**
- Server stores **no session data**
- Server only **verifies the token cryptographically**

JWT fits REST principles perfectly.

---

## 3. JWT Structure — The 3 Parts

A JWT always has **three Base64URL-encoded parts**, separated by dots:

```
header.payload.signature
```

Example (real token from our demo):

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
.
eyJzdWIiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwibmJmIjoxNzY2MzIzMzE0LCJleHAiOjE3NjYzMjY5MTQsImlzcyI6IlJlc3RmdWxBcGlUcmFpbmluZyIsImF1ZCI6IlJlc3RmdWxBcGlUcmFpbmluZ0NsaWVudHMifQ
.
rvZiAaSPeryiNvqrcYc_KFDPwFtArl5ImGtm3lkhx88
```

---

## 4. Base64 Encoding — What It Is and What It Is NOT

### What Base64 Is
- An **encoding**, not encryption
- Converts binary data → text
- Fully reversible

### What Base64 Is NOT
- ❌ Not encryption
- ❌ Not security
- ❌ Not validation

Anyone can decode a JWT header and payload.  
- https://www.base64encode.org/  
- https://www.base64decode.org/  

**Security comes from the signature.**

---

## 5. Decoding JWT Using jwt.io

For training and debugging, use:

👉 **https://www.jwt.io**

What jwt.io does:
- Base64URL-decodes header and payload
- Shows claims in readable JSON
- Verifies signature **only if you provide the secret**

⚠️ **Never paste production secrets into jwt.io**  
For this training, a **demo dev key** is intentionally used.

---

## 6. Decoded JWT — Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

| Field | Meaning |
|-----|--------|
| `alg` | HMAC SHA-256 (symmetric signing) |
| `typ` | Token type |

---

## 7. Decoded JWT — Payload (Claims)

```json
{
  "sub": "1",
  "unique_name": "admin",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "admin",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
  "nbf": 1766323314,
  "exp": 1766326914,
  "iss": "RestfulApiTraining",
  "aud": "RestfulApiTrainingClients"
}
```

---

## 8. JWT Claims Explained

| Claim | Meaning |
|-----|--------|
| `sub` | User ID |
| `unique_name` | Username |
| `nameidentifier` | .NET identity mapping |
| `name` | Display name |
| `role` | Used for authorization policies |
| `nbf` | Not valid before |
| `exp` | Expiry time |
| `iss` | Issuer |
| `aud` | Audience |

Authorization policies work **only from these claims**, not DB calls.

---

## 9. How JWT Is Created (High-Level Flow)

```
1. Client sends username + password
2. Server validates credentials using DB
3. Server builds claims
4. Server builds header (alg, typ)
5. Server Base64URL-encodes header & payload
6. Server signs header.payload using secret key
7. Token is returned to client
```

---

## 10. How JWT Is Used in Requests

For protected APIs, the client sends:

```
Authorization: Bearer <JWT_TOKEN>
```

On every request:
- No session lookup
- No database hit
- Only cryptographic verification

---

## 11. HOW JWT SIGNATURE VALIDATION ACTUALLY WORKS  
*(Critical Concept)*

❗ **JWT validation does NOT compare Base64 payloads.**

Base64 is only for transport and readability.

### JWT signature protects:
- **Authenticity** (token was issued by server)
- **Integrity** (token was not modified)

---

### Step-by-step signature validation (HS256)

JWT format:
```
H.P.S
```

Where:
- `H` = Base64URL(header)
- `P` = Base64URL(payload)
- `S` = Base64URL(signature)

#### Step 1: Split the token
```
H, P, S = token.split(".")
```

#### Step 2: Recompute expected signature
```
message = H + "." + P
expectedSignature =
    Base64URL(
        HMAC_SHA256(secretKey, message)
    )
```

#### Step 3: Compare signatures
```
if expectedSignature != S:
    reject token (401 Unauthorized)
```

If they match → token is **valid and untampered**.

---

### Why this works

- Anyone can Base64-decode payload
- **Only the server** knows the secret key
- Any payload change breaks the signature

Example attack attempt:
```json
"role": "Admin"
```
→ modified payload  
→ signature mismatch  
→ request rejected

---

### Tiny pseudo-code (close to reality)

```text
token = "H.P.S"
(H, P, S) = split(token)

expected = base64url(
    HMACSHA256(secretKey, H + "." + P)
)

if expected != S:
    reject

payload = base64url_decode(P)
validate exp, nbf, iss, aud
build ClaimsPrincipal
authorize request
```

---

## 12. HS256 vs RS256 (Quick Note)

| Algorithm | Key Type |
|---------|---------|
| HS256 | One shared secret |
| RS256 | Private key signs, public key verifies |

This project uses **HS256**.

---

## 13. What Is Basic Authentication?

Basic Authentication format:
```
Authorization: Basic base64(username:password)
```

Example:
```
admin:admin
↓
YWRtaW46YWRtaW4=
```

---

## 14. How Basic Auth Travels Over HTTP

- Username and password are:
  - Base64-encoded
  - Sent **on every request**

Important:
- Base64 ≠ encryption
- HTTPS is mandatory
- Credentials repeatedly traverse the network

---

## 15. Basic Auth vs JWT — Comparison

| Aspect | Basic Auth | JWT |
|------|-----------|-----|
| Password sent | Every request | Once |
| Stateless | Yes | Yes |
| Token expiry | No | Yes |
| Scalability | Poor | Excellent |
| Modern usage | Rare | Standard |

In this project:
- Same DB
- Same user
- Same password hash
- Different transport model

---

## 16. Live Demo Flow (Postman)

### 1️⃣ Unprotected API
```
GET /weatherforecast
```

### 2️⃣ JWT Login
```
POST /api/auth/login
```

### 3️⃣ JWT-Protected API
```
GET /api/users/id/1
Authorization: Bearer <JWT>
```

### 4️⃣ Basic Auth-Protected API
```
GET /api/auth/basic-demo
Authorization: Basic <credentials>
```

---

## 17. Final Takeaways

- JWT ≠ encrypted payload
- Base64 ≠ security
- Signature = trust
- JWT enables stateless, scalable APIs
- Basic Auth is simple but risky
- Understanding **what is validated** is critical

---

## 18. Recommended Tools

- **Postman**
- **https://www.jwt.io**
- **Swagger / OpenAPI**

---

### End of Training Material
