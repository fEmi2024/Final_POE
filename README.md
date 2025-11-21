# Final_POE

PROG6121: Lecturer Claim Automation System (L-CAS)

Project Overview

The Lecturer Claim Automation System (L-CAS) serves as the Portfolio of Evidence (POE) for PROG6121. This application is designed to digitize and automate the lifecycle of lecturer payment claims, encompassing submission, rigorous validation, mandatory management verification, and final payroll reporting. The system is built on the ASP.NET Core MVC framework using C# and Entity Framework Core. A core principle of the design is strict adherence to the Model-View-Controller (MVC) architectural pattern, maintaining a strong Separation of Concerns. All critical business logic, especially for the functional and distinction requirements, is housed within a dedicated Service Layer to maximize testability and maintainability.

Technology Stack

The application is implemented using the ASP.NET Core MVC framework. It leverages C# for all backend logic and uses Entity Framework Core with a Code-First approach for data persistence. XUnit is the framework used for all unit testing, ensuring the validation of business rules and automated processes.

Functional Requirements

F1: Automatic Calculation

The system automatically calculates the Final Payment Amount immediately upon claim submission. This calculation is derived by multiplying the Hours Claimed by the defined Hourly Rate. The logic for this critical financial calculation is isolated and executed within the ClaimService.

F2: Manager Flagging

To manage financial risk, any claim exceeding a predefined threshold of 160 hours is automatically flagged as High-Risk. This process sets a boolean flag (RequiresManagerFlag = true) on the claim, making mandatory manager verification a required step before the claim can proceed to final approval.

F3: Claim Submission

Lecturers are provided with a secure, validated web form for submitting claims. The input fields are protected using Data Annotations to enforce all necessary data constraints and ensure the integrity of the submitted information.

F4: Claim Tracking

The system allows lecturers to view and track their entire history of submitted claims. This includes the ability to view all claims and to filter them by their current status (Pending, Approved, Rejected). The lecturer's email address is used as the primary identifier for securely accessing their personal claim history.

Distinction Requirement: P3 (Process Automation)

The Distinction Requirement P3 (Process Automation) is proven through two sophisticated, fully automated workflows designed to support the Coordinator role and enforce financial compliance.

Priority Queueing System for Verification

The system automates the verification queue for managers. Claims automatically flagged as High-Risk (RequiresManagerFlag = true) are immediately prioritized and displayed at the top of the manager's list, ensuring they receive immediate attention. Within both the High-Risk and Standard groups, claims are sorted by the oldest submission date first (FIFO), ensuring an efficient, time-based processing flow. This prioritization logic is validated by the P3_Priority_Queue_Test unit test.

Payroll Exclusion Logic

To maintain strict financial integrity, the final summation report generated for the Human Resources (HR) department adheres to a payroll exclusion rule. This process automatically ensures that only claims with the explicit status of ClaimStatus.Approved are included in the final payout summation. This prevents the accidental payment of claims that are still pending verification or have been formally rejected. This vital exclusion logic is validated by the P3_Payroll_Summation_Exclusion_Test unit test.

Unit Testing Validation

All core business logic and automation rules are rigorously validated using XUnit tests. Key validation tests include F1_F2_Logic_Test, F2_Process_Decision_Test, and the distinction requirement validation tests: P3_Priority_Queue_Test and P3_Payroll_Summation_Exclusion_Test.



