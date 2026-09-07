# Certus

## AI-Native Quantitative Research Organization

Certus is an AI-native quantitative research system designed to bridge the gap between independent traders and institutional quantitative firms.

The goal of Certus is not to build a simple trading bot. Instead, Certus aims to provide a systematic research environment where humans and AI agents collaborate to discover, validate, manage, and evolve quantitative trading strategies.

By combining AI-assisted research workflows, systematic experimentation, knowledge accumulation, and risk-aware validation, Certus seeks to bring institutional-style quantitative processes to independent traders and small quantitative teams.

---

## Vision

Professional quantitative firms operate through structured research processes involving:

- quantitative researchers
- data engineers
- software engineers
- risk specialists
- continuous experimentation

Independent traders rarely have access to this level of organization.

Certus aims to reduce this gap by building an AI-native quantitative research organization where specialized agents assist humans throughout the strategy lifecycle.

Long term, Certus aims to evolve into a next-generation quantitative investment organization powered by human expertise, AI agents, and accumulated research knowledge.

---

## Core Philosophy

### Research Before Trading

Every trading strategy starts as a hypothesis.

A hypothesis must be:

- documented
- tested
- validated
- challenged

before becoming a candidate for deployment.

---

### Knowledge Is the Primary Asset

The long-term advantage of Certus is not individual trading signals.

The core asset is accumulated quantitative knowledge:

- successful experiments
- failed experiments
- validation results
- market observations
- strategy evolution history

---

### Progressive Autonomy

Certus evolves gradually from human-supervised workflows toward higher levels of AI autonomy.

Initial phases prioritize:

- human decision-making
- transparent workflows
- explainable results
- controlled experimentation

AI agents progressively take responsibility for:

- research assistance
- hypothesis generation
- analysis
- documentation
- validation support

---

## Initial Scope

Certus is designed as a market-agnostic quantitative research platform.

The first implementation focuses on Forex markets because of:

- accessible historical data
- availability of research tools
- lower infrastructure requirements
- suitability for systematic experimentation

Future expansion may include:

- equities
- futures
- commodities
- digital assets
- other quantitative finance domains

---

## Current Development Phase

Certus is currently in the foundation phase.

The initial objective is to build a Quantitative Research Operating System capable of supporting repeatable trading experiments.

Initial capabilities include:

- research workflow management
- experiment tracking
- strategy lifecycle management
- backtest result analysis
- quantitative knowledge management

Early strategy development may use existing tools such as StrategyQuant X while Certus gradually introduces AI-assisted capabilities into the research lifecycle.

---

## Architecture Direction
``` 
The long-term architecture follows an AI-native collaborative model:

             Certus
                |
    +-----------+-----------+
    |           |           |
Research    Experiment  Knowledge
Agents        Engine       Base
                |
                v
        Strategy Lifecycle
                |
                v
          Risk Validation
                |
                v
            Deployment
		 
``` 
---

## Non-Goals

Certus does not aim to:

- guarantee trading profits
- perfectly predict financial markets
- create black-box trading systems
- replace human judgment immediately
- focus on high-frequency trading in early stages
- optimize strategies only based on historical backtests

---

## Development Principles

- Start small, think big
- Build incrementally
- Validate before scaling
- Prefer evidence over assumptions
- Automate processes gradually
- Convert knowledge into reusable assets

---

## Status

🚧 Early Development

Certus is currently being developed as an experimental AI-native quantitative research platform.

## Data Validation

Certus integrates the [financial-data-cleaner](https://github.com/MehdiDolati/financial-data-cleaner) validator so users can check OHLCV datasets directly from the dashboard.

- **Validation page** (`/validation`): upload a CSV dataset, optionally pin the timeframe and request a quality score. Duplicate submissions deterministically join the existing run.
- **Run detail page** (`/validation/runs/{id}`): the durable run status, the six quality-check counts, the optional score breakdown, and report downloads (concise text, detailed text, and v2 JSON) for terminal-success runs.
- **Integration boundary**: consumed as NuGet packages from the validator's local feed (`nuget.config`, `validator-local` source). The boundary is transport-neutral; Certus wires it in `Program.cs` via `AddValidatorWebIntegration` with its durable storage root under `AppData/data-validation`.
- **Parity**: the validator's parity test suite proves the web results match the CLI byte-for-byte on the substantive surfaces, so dashboard users see exactly what CLI users see.

The validator boundary never publishes partial results: every run is Pending, Running, CompletedClean/CompletedWithFindings, or Failed with an actionable diagnostic.