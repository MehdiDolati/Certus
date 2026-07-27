# Certus Documentation

Welcome to the Certus documentation.

This documentation describes the vision, design, development process,
and evolution of Certus.

If you are new to the project, start here.

---

# Getting Started

## 1. Understand Why Certus Exists

Start with:

- [Product Vision](product/vision.md)

This explains the long-term purpose and direction of Certus.

---

## 2. Understand Product Direction

Read:

- [Business Goals](product/business-goals.md)
- [Product Principles](product/principles.md)
- [Capabilities](product/capabilities.md)

These documents define what Certus should achieve and the principles
guiding decisions.

---

## 3. Understand the MVP

Read:

- [MVP Scope](product/mvp-scope.md)
- [Feature Map](product/feature-map.md)

These documents define the first implementation scope.

---

## 4. Understand the Domain

Read:

- [Domain Model](domain/domain-model.md)

This describes the core concepts of Certus:

- research
- experiments
- strategies
- validation
- monitoring
- knowledge lifecycle

---

# Development

Certus follows a specification-driven development approach.

Before implementation:
Feature

↓

Specification

↓

Implementation

↓

Validation

↓

Artifact Update

Development process documentation:

- [SDD Workflow](development/sdd-workflow.md)

---

# Features

Feature specifications are maintained here:

- [Feature Specifications](features/README.md)

Each feature specification defines:

- user value
- requirements
- acceptance criteria
- technical considerations

---

# Architecture

Architecture decisions and system design:

- [Architecture Documentation](architecture/README.md)

Includes:

- system overview
- architectural decisions
- trade-offs

---

# Research

Research notes, experiments, and explorations:
research/
This area contains supporting knowledge that may influence future
decisions.

---

# Documentation Principles

Certus documentation follows these principles:

## Single Source of Truth

Important decisions must exist in the repository, not only in conversations.

---

## Traceability

Every implementation decision should be traceable back to:

- business goal
- capability
- feature specification

---

## Incremental Evolution

Documentation evolves together with the system.

The goal is not to create documentation before building.

The goal is to create the right documentation at the right time.