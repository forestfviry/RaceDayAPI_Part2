/* ============================================================
   Event Registration & Tracking System
   Corrected schema — adds the Result entity that was missing
   from the diagram (Entry 1 --- 0..1 Result).
   Dialect: T-SQL (SQL Server)
   ============================================================ */

CREATE TABLE [User] (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255)   NOT NULL,
    Role            NVARCHAR(20)    NOT NULL
                    CHECK (Role IN ('Organiser', 'Participant')),
    PhoneNumber     NVARCHAR(20)    NULL,
    DateRegistered  DATETIME2       NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Route (
    RouteID         INT IDENTITY(1,1) PRIMARY KEY,
    RouteName       NVARCHAR(100)   NOT NULL,
    DistanceKM      DECIMAL(6,2)    NOT NULL,
    ElevationGain   DECIMAL(6,2)    NULL,
    StartLocation   NVARCHAR(200)   NULL,
    EndLocation     NVARCHAR(200)   NULL,
    MapFilePath     NVARCHAR(255)   NULL
);

CREATE TABLE Event (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID     INT             NOT NULL,
    EventName       NVARCHAR(150)   NOT NULL,
    EventDate       DATE            NOT NULL,
    Location        NVARCHAR(200)   NULL,
    Description     NVARCHAR(MAX)   NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Planned',
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (OrganiserID)
        REFERENCES [User](UserID)
);

CREATE TABLE Category (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT             NOT NULL,
    RouteID         INT             NOT NULL,
    CategoryName    NVARCHAR(100)   NOT NULL,
    DistanceKM      DECIMAL(6,2)    NOT NULL,   -- can differ from Route.DistanceKM
    EntryFee        DECIMAL(8,2)    NOT NULL DEFAULT 0,
    MaxParticipants INT             NULL,
    CONSTRAINT FK_Category_Event FOREIGN KEY (EventID)
        REFERENCES Event(EventID),
    CONSTRAINT FK_Category_Route FOREIGN KEY (RouteID)
        REFERENCES Route(RouteID)
);

CREATE TABLE Entry (
    EntryID         INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID   INT             NOT NULL,
    CategoryID      INT             NOT NULL,
    RegistrationDate DATETIME2      NOT NULL DEFAULT GETDATE(),
    PaymentStatus   NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
    EntryStatus     NVARCHAR(20)    NOT NULL DEFAULT 'Registered',
    CONSTRAINT FK_Entry_Participant FOREIGN KEY (ParticipantID)
        REFERENCES [User](UserID),
    CONSTRAINT FK_Entry_Category FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID),

    CONSTRAINT UQ_Entry_Participant_Category UNIQUE (ParticipantID, CategoryID)
        -- stops the same participant double-entering one category
);

CREATE TABLE Weather (
    WeatherID       INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT             NOT NULL,
    RecordedAt      DATETIME2       NOT NULL DEFAULT GETDATE(),
    Temperature     DECIMAL(4,1)    NULL,
    Condition       NVARCHAR(50)    NULL,
    WindSpeed       DECIMAL(5,1)    NULL,
    Humidity        DECIMAL(4,1)    NULL,
    CONSTRAINT FK_Weather_Event FOREIGN KEY (EventID)
        REFERENCES Event(EventID)
);

/* -------------------------------------------------------------
   Result — the entity missing from the original diagram.
   EntryID is UNIQUE, which is what actually enforces the
   1 : 0..1 cardinality from business rule 6 (one Entry produces
   at most one Result). Without that UNIQUE, it'd default to 1:M.
   ------------------------------------------------------------- */
CREATE TABLE Result (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EntryID         INT             NOT NULL UNIQUE,
    FinishTime      TIME            NULL,
    Position        INT             NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Completed'
                    CHECK (Status IN ('Completed', 'DNF', 'DQ')),
    CONSTRAINT FK_Result_Entry FOREIGN KEY (EntryID)
        REFERENCES Entry(EntryID)
);