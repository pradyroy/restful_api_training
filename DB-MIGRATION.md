# Database Migration with Flyway — Practical Training Guide  
*(Using MySQL + Flyway Community)*

This document is a **hands-on training guide** to understand and demonstrate:

- Why **database migrations** are necessary
- What problems migrations solve
- How **Flyway** manages schema evolution
- How **versioned SQL migrations** work
- A **live demo** with multiple migration versions
- How to **reset a database safely** in DEV / training
- A brief **history of database migrations in modern software development**

This guide is intentionally **simple, practical, and production-aligned**.

---

## 1. A Short History of Database Migrations

### Phase 1: Manual SQL Era (Early 2000s)
In early enterprise applications:
- Databases were changed manually
- DBAs ran ad-hoc `CREATE` / `ALTER` scripts
- Changes were shared via email or documents

Problems:
- No ordering
- No audit trail
- High risk of missed or duplicated changes
- Environments drifted quickly

---

### Phase 2: Script Folders & Wiki Pages
Teams began:
- Storing SQL scripts in shared folders
- Maintaining “run these scripts in order” wiki pages

Problems:
- Humans enforced ordering
- Scripts were re-run accidentally
- No reliable record of what ran where

---

### Phase 3: Migration Tools
Dedicated migration tools emerged:
- SQL-first
- Versioned
- Repeatable
- CI/CD friendly

Key shift:
> **Database schema became a first-class, versioned artifact.**

Flyway belongs to this phase.

---

### 1.1 Industry Timeline

2005–2006  → Rails migrations (framework-bound)  
2006       → Liquibase (first general DB migration tool)  
2010       → Flyway  
2015+      → Database migrations become standard DevOps practice  

---

### 1.2 Redgate — Timeline

1999        → Redgate founded, focused on SQL Server and DBA tooling  
2000–2005   → Database comparison and deployment tools gain adoption  
2006–2010   → Emphasis on controlled, repeatable database change management  
2010        → SQL Source Control released, integrating source control into SSMS  
2010–2014   → CI/CD adoption exposes databases as a deployment bottleneck  
2015–2019   → Shift toward Database DevOps and automation  
2021        → Redgate acquires Flyway, embracing SQL-first migrations  
2022+       → Flyway positioned as Redgate’s core migration & DevOps platform  

---

### 1.3 DACPAC, BACPAC and SQL Source Control (State-Based Tools)

Before migration-based workflows became common, SQL Server teams needed ways to **package, version, and deploy database schemas safely**.  
Microsoft introduced **DACPAC** and **BACPAC**, while Redgate introduced **SQL Source Control**, to address this need.

**DACPAC (Data-tier Application Package)**
- Contains **database schema only**
- Represents the **desired end state** of a database
- Used for **schema deployment and comparison**
- **State-based**, not version-based
- Common in **build and release pipelines**

**BACPAC (Backup Package)**
- Contains **schema + data**
- Used for **database export, transfer, and archiving**
- Also **state-based**

**SQL Source Control**
- Integrates directly with **SSMS**
- Versions **live database objects** into source control
- Optimized for **developer and DBA workflows**
- **State-based**, using diff/compare

**Key distinction**
> **DACPAC and SQL Source Control capture database state;  
migration tools manage database history.**

This explains why state-based tools appeared earlier, and why migration-based tools later became essential for modern DevOps workflows.

---

## 2. Why Database Migrations Exist

Database migrations answer one question:

> **“How does the database schema change safely over time?”**

In real systems:
- Tables evolve
- Columns are added
- Data must be seeded
- Multiple environments exist

Without migrations:
- Schemas drift
- Environments differ
- Deployments break

---

## 3. What Is Flyway?

**Flyway** is a database migration tool that:

- Applies schema changes in a **defined order**
- Tracks what ran and when
- Prevents accidental re-execution
- Makes database changes **repeatable and auditable**

Key idea:

> **Database schema changes are versioned like code.**

---

## 4. Flyway Core Concepts

Flyway works using:

- **Versioned SQL files**
- **Strict ordering**
- **A schema history table**
- **Forward-only evolution**

No runtime magic. Just controlled SQL.

---

## 5. Migration Naming Convention

Flyway migration files follow this format:

```
V<version>__<description>.sql
```

Examples:
```
V1__create_table1.sql
V2__create_table2.sql
V3__create_table3.sql
V4__seed_demo_data.sql
```

Rules:
- Versions are immutable
- Applied migrations must never change
- Fixes are done using new versions

---

## 6. Demo Schema Overview

We demonstrate **incremental schema evolution**.

### V1 — Table1
- `id` (BIGINT, PK)
- `col11` (VARCHAR 10)

### V2 — Table2
- `id` (BIGINT, PK)
- `col21` (VARCHAR 10)
- `col22` (VARCHAR 20)

### V3 — Table3
- `id` (BIGINT, PK)
- `col31` (VARCHAR 10)
- `col32` (VARCHAR 20)
- `col33` (VARCHAR 30)

### V4 — Seed Data
- Inserts demo rows
- Versioned like schema changes

---

## 7. Project Structure

```
db/
 └─ migrations/
     ├─ V1__create_table1.sql
     ├─ V2__create_table2.sql
     ├─ V3__create_table3.sql
     └─ V4__seed_demo_data.sql
flyway.conf
```

Flyway lives **alongside the project**, not inside application code.

---

## 8. Flyway Configuration (MySQL)

```properties
flyway.url=jdbc:mysql://localhost:3306/restful_api_training
flyway.user=root
flyway.password=pa55word
flyway.locations=filesystem:db/migrations
```

Flyway uses JDBC and runs **outside the application runtime**.

---

## 9. Schema History Table (Very Important)

Flyway automatically creates:

```
flyway_schema_history
```

This table records:
- Version
- Description
- Execution status
- Timestamp
- Checksum

This table is:
- The migration authority
- Never edited manually
- Used to prevent drift

---

## 10. Running Migrations

```bash
flyway migrate
```

---

## 11. Step-by-Step Migration (Training Mode)

```bash
flyway -target=1 migrate
flyway -target=2 migrate
flyway -target=3 migrate
flyway -target=4 migrate
```

---

## 12. Forward-Only Migrations

Flyway Community is **forward-only**:

- ❌ No rollback
- ❌ No downgrade
- ✅ Fix mistakes using a new migration

---

## 13. Resetting the Database (DEV / TRAINING ONLY)

```bash
flyway clean
flyway migrate
```

⚠️ Never use `clean` in production.

---

## 14. Final Takeaways

- Database schema changes must be versioned
- SQL migrations are deterministic
- Forward-only evolution is safer
- Flyway supports modern DevOps workflows
- Repeatability builds confidence

---

### End of Training Material
