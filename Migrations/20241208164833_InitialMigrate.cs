using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyField.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CookiePreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PerformanceCookies = table.Column<bool>(type: "bit", nullable: false),
                    FunctionalityCookies = table.Column<bool>(type: "bit", nullable: false),
                    TargetingCookies = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookiePreferences", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceInfo",
                columns: table => new
                {
                    DeviceInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OSName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OSVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrowserVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceInfo", x => x.DeviceInfoId);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false),
                    ManagerFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DivisionBadge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DivisionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionAbbr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionType = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingRequests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "PayFastWebhookRecords",
                columns: table => new
                {
                    PayFastWebhookPayloadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayFastWebhookRecords", x => x.PayFastWebhookPayloadId);
                });

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "TestUserFeedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestUserFeedbacks", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    ActivityLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserBaseModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceInfoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.ActivityLogId);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_DeviceInfo_DeviceInfoId",
                        column: x => x.DeviceInfoId,
                        principalTable: "DeviceInfo",
                        principalColumn: "DeviceInfoId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnouncementId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    IsFirstTimeLogin = table.Column<bool>(type: "bit", nullable: true),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    ClubManager_ClubId = table.Column<int>(type: "int", nullable: true),
                    IsContractEnded = table.Column<bool>(type: "bit", nullable: true),
                    ClubManager_DivisionId = table.Column<int>(type: "int", nullable: true),
                    DivisionManager_DivisionId = table.Column<int>(type: "int", nullable: true),
                    Officials_DivisionId = table.Column<int>(type: "int", nullable: true),
                    MarketValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    JerseyNumber = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsContractSigned = table.Column<bool>(type: "bit", nullable: true),
                    PlayerCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Player_ClubId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: true),
                    HasPlayed = table.Column<bool>(type: "bit", nullable: true),
                    IsOnPitch = table.Column<bool>(type: "bit", nullable: true),
                    Player_DivisionId = table.Column<int>(type: "int", nullable: true),
                    SportsMember_DivisionId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommenterId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CommentDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_CommentById",
                        column: x => x.CommentById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Competition",
                columns: table => new
                {
                    CompetitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WinnerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NumberOfParticipants = table.Column<int>(type: "int", nullable: false),
                    CompetitionStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competition", x => x.CompetitionId);
                    table.ForeignKey(
                        name: "FK_Competition_AspNetUsers_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    DivisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionBadge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DivisionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionAbbr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.DivisionId);
                    table.ForeignKey(
                        name: "FK_Divisions_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Divisions_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Formations",
                columns: table => new
                {
                    FormationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormationImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formations", x => x.FormationId);
                    table.ForeignKey(
                        name: "FK_Formations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Formations_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Maintainances",
                columns: table => new
                {
                    MaintainanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaintainanceDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    maintainanceRequestStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintainances", x => x.MaintainanceId);
                    table.ForeignKey(
                        name: "FK_Maintainances_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Maintainances_AspNetUsers_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TermsAggreements",
                columns: table => new
                {
                    TermsAggreementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserBaseModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsAggreed = table.Column<bool>(type: "bit", nullable: false),
                    AggreementTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsAggreements", x => x.TermsAggreementId);
                    table.ForeignKey(
                        name: "FK_TermsAggreements_AspNetUsers_UserBaseModelId",
                        column: x => x.UserBaseModelId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tournament",
                columns: table => new
                {
                    TournamentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TournamentOrgarnizer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoiningFee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentRules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournament", x => x.TournamentId);
                    table.ForeignKey(
                        name: "FK_Tournament_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Tournament_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                columns: table => new
                {
                    WarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserBaseModelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfOffences = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_Warnings_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Warnings_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Warnings_AspNetUsers_UserBaseModelId",
                        column: x => x.UserBaseModelId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionParticipants",
                columns: table => new
                {
                    CompetitionParticipantsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionParticipants", x => x.CompetitionParticipantsId);
                    table.ForeignKey(
                        name: "FK_CompetitionParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CompetitionParticipants_Competition_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competition",
                        principalColumn: "CompetitionId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DivisionAggreements",
                columns: table => new
                {
                    AggreementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    SignedContract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgreementStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgreementEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionAggreements", x => x.AggreementId);
                    table.ForeignKey(
                        name: "FK_DivisionAggreements_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DivisionAggreements_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DivisionAggreements_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    LeagueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueYears = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LeagueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_League", x => x.LeagueId);
                    table.ForeignKey(
                        name: "FK_League_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_League_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_League_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                });

            migrationBuilder.CreateTable(
                name: "Meeting",
                columns: table => new
                {
                    MeetingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeetingDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeetingTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MeetingAttendees = table.Column<int>(type: "int", nullable: false),
                    MeetingStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meeting", x => x.MeetingId);
                    table.ForeignKey(
                        name: "FK_Meeting_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Meeting_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Meeting_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                });

            migrationBuilder.CreateTable(
                name: "SportNew",
                columns: table => new
                {
                    NewsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsHeading = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthoredById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PublishedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RejectedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    RejectedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewsBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewsImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewsStatus = table.Column<int>(type: "int", nullable: false),
                    ReasonForReEdit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportNew", x => x.NewsId);
                    table.ForeignKey(
                        name: "FK_SportNew_AspNetUsers_AuthoredById",
                        column: x => x.AuthoredById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SportNew_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SportNew_AspNetUsers_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SportNew_AspNetUsers_RejectedById",
                        column: x => x.RejectedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SportNew_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                });

            migrationBuilder.CreateTable(
                name: "Club",
                columns: table => new
                {
                    ClubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubBadge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubAbbr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    ClubCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClubManagerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.ClubId);
                    table.ForeignKey(
                        name: "FK_Club_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Club_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Club_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Club_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TopAssists",
                columns: table => new
                {
                    TopAssistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumberOfAssists = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopAssists", x => x.TopAssistId);
                    table.ForeignKey(
                        name: "FK_TopAssists_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TopAssists_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_TopAssists_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TopScores",
                columns: table => new
                {
                    TopScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumberOfGoals = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopScores", x => x.TopScoreId);
                    table.ForeignKey(
                        name: "FK_TopScores_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TopScores_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_TopScores_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TransferPeriod",
                columns: table => new
                {
                    TransferPeriodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    PeriodOpenCount = table.Column<int>(type: "int", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsOpened = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferPeriod", x => x.TransferPeriodId);
                    table.ForeignKey(
                        name: "FK_TransferPeriod_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TransferPeriod_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TransferPeriod_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_TransferPeriod_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ClubWarnings",
                columns: table => new
                {
                    WarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CLubId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfOffences = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubWarnings", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_ClubWarnings_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ClubWarnings_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ClubWarnings_Club_CLubId",
                        column: x => x.CLubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    FineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleViolated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FineDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    OffenderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FineDuDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.FineId);
                    table.ForeignKey(
                        name: "FK_Fines_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fines_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fines_AspNetUsers_OffenderId",
                        column: x => x.OffenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Fines_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId");
                    table.ForeignKey(
                        name: "FK_Fines_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                });

            migrationBuilder.CreateTable(
                name: "Fixture",
                columns: table => new
                {
                    FixtureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    KickOffDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KickOffTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FixtureStatus = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixture", x => x.FixtureId);
                    table.ForeignKey(
                        name: "FK_Fixture_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fixture_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fixture_Club_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fixture_Club_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Fixture_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Fixture_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "HeadToHead",
                columns: table => new
                {
                    HeadToHeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    MatchResults = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeTeamGoals = table.Column<int>(type: "int", nullable: false),
                    AwayTeamGoals = table.Column<int>(type: "int", nullable: false),
                    HeadToHeadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadToHead", x => x.HeadToHeadId);
                    table.ForeignKey(
                        name: "FK_HeadToHead_Club_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HeadToHead_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HeadToHead_Club_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMadeById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DeviceInfoId = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_PaymentMadeById",
                        column: x => x.PaymentMadeById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId");
                    table.ForeignKey(
                        name: "FK_Payments_DeviceInfo_DeviceInfoId",
                        column: x => x.DeviceInfoId,
                        principalTable: "DeviceInfo",
                        principalColumn: "DeviceInfoId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Payments_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                });

            migrationBuilder.CreateTable(
                name: "PlayerTransferMarket",
                columns: table => new
                {
                    PlayerTransferMarketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleStatus = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    ArchivedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTransferMarket", x => x.PlayerTransferMarketId);
                    table.ForeignKey(
                        name: "FK_PlayerTransferMarket_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PlayerTransferMarket_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PlayerTransferMarket_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PlayerTransferMarket_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_PlayerTransferMarket_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Standing",
                columns: table => new
                {
                    StandingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    MatchPlayed = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Lose = table.Column<int>(type: "int", nullable: false),
                    GoalsScored = table.Column<int>(type: "int", nullable: false),
                    GoalsConceded = table.Column<int>(type: "int", nullable: false),
                    GoalDifference = table.Column<int>(type: "int", nullable: false),
                    Draw = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Last5Games = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standing", x => x.StandingId);
                    table.ForeignKey(
                        name: "FK_Standing_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Standing_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Standing_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Standing_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Standing_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionPlan = table.Column<int>(type: "int", nullable: false),
                    SubscriptionStatus = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId");
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    ClubPerformanceReport_LeagueId = table.Column<int>(type: "int", nullable: true),
                    ClubPerformanceReport_ClubId = table.Column<int>(type: "int", nullable: true),
                    GamesToPlayCount = table.Column<int>(type: "int", nullable: true),
                    GamesPlayedCount = table.Column<int>(type: "int", nullable: true),
                    GamesNotPlayedCount = table.Column<int>(type: "int", nullable: true),
                    GamesWinCount = table.Column<int>(type: "int", nullable: true),
                    GamesLoseCount = table.Column<int>(type: "int", nullable: true),
                    GamesDrawCount = table.Column<int>(type: "int", nullable: true),
                    GamesPlayedRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GamesNotPlayedRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GamesWinRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GamesLoseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GamesDrawRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClubTransferReport_LeagueId = table.Column<int>(type: "int", nullable: true),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    OverallTransfersCount = table.Column<int>(type: "int", nullable: true),
                    OutgoingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    IncomingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    OutgoingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IncomingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuccessfulOutgoingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    SuccessfulIncomingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    RejectedOutgoingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    RejectedIncomingTransfersCount = table.Column<int>(type: "int", nullable: true),
                    NotActionedIncomingTransferCount = table.Column<int>(type: "int", nullable: true),
                    NotActionedOutgoigTransferCount = table.Column<int>(type: "int", nullable: true),
                    SuccessfullIncomingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejectedOutgoingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuccessfullOutgoingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejectedIncomingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotActionedIncomingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotActionedOutgoingTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OverallFansAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveFansAccountsCount = table.Column<int>(type: "int", nullable: true),
                    InactiveFansAccountsCount = table.Column<int>(type: "int", nullable: true),
                    SuspendedFansAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveFansAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InactiveFansAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuspendedFansAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReadersCount = table.Column<int>(type: "int", nullable: true),
                    SportNewsId = table.Column<int>(type: "int", nullable: true),
                    MatchReports_LeagueId = table.Column<int>(type: "int", nullable: true),
                    MatchesToBePlayedCount = table.Column<int>(type: "int", nullable: true),
                    FixturedMatchesCount = table.Column<int>(type: "int", nullable: true),
                    UnreleasedFixturesCount = table.Column<int>(type: "int", nullable: true),
                    PlayedMatchesCounts = table.Column<int>(type: "int", nullable: true),
                    PostponedMatchesCount = table.Column<int>(type: "int", nullable: true),
                    InterruptedMatchesCount = table.Column<int>(type: "int", nullable: true),
                    FixturedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnfixturedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PlayedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PostponedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InterruptedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MatchResultsReports_LeagueId = table.Column<int>(type: "int", nullable: true),
                    ExpectedResultsCount = table.Column<int>(type: "int", nullable: true),
                    ReleasedResultsCount = table.Column<int>(type: "int", nullable: true),
                    UnreleasedResultsCount = table.Column<int>(type: "int", nullable: true),
                    WinsCount = table.Column<int>(type: "int", nullable: true),
                    LosesCount = table.Column<int>(type: "int", nullable: true),
                    DrawsCount = table.Column<int>(type: "int", nullable: true),
                    ReleasedResultsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnreleasedMatchesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WinningRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LosingRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DrawingRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OnboardingRequestsTotalCount = table.Column<int>(type: "int", nullable: true),
                    ApprovedRequestsCount = table.Column<int>(type: "int", nullable: true),
                    ApprovedRequestsRate = table.Column<double>(type: "float", nullable: true),
                    RejectedRequestsCount = table.Column<int>(type: "int", nullable: true),
                    RejectedRequestsRate = table.Column<double>(type: "float", nullable: true),
                    CompletedRequestsCount = table.Column<int>(type: "int", nullable: true),
                    CompletedRequestsRate = table.Column<double>(type: "float", nullable: true),
                    AuthoredNewsCount = table.Column<int>(type: "int", nullable: true),
                    ApprovedNewsCount = table.Column<int>(type: "int", nullable: true),
                    PublishedNewsCount = table.Column<int>(type: "int", nullable: true),
                    RejectedNewsCount = table.Column<int>(type: "int", nullable: true),
                    NewsReadersCount = table.Column<int>(type: "int", nullable: true),
                    ApprovedNewsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PublishedNewsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejectedNewsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OverallAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveAccountsCount = table.Column<int>(type: "int", nullable: true),
                    InactiveAccountsCount = table.Column<int>(type: "int", nullable: true),
                    SuspendedAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InactiveAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuspendedAccountsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpectedRepayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalUnpaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RepayableFinesCount = table.Column<int>(type: "int", nullable: true),
                    PaidPaymentFinesCount = table.Column<int>(type: "int", nullable: true),
                    PendingPaymentFinesCount = table.Column<int>(type: "int", nullable: true),
                    OverduePaymentFineCount = table.Column<int>(type: "int", nullable: true),
                    PaidFinesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PendingFinesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OverdueFinesRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AppearancesCount = table.Column<int>(type: "int", nullable: true),
                    GoalsScoredCount = table.Column<int>(type: "int", nullable: true),
                    AssistsCount = table.Column<int>(type: "int", nullable: true),
                    YellowCardCount = table.Column<int>(type: "int", nullable: true),
                    RedCardCount = table.Column<int>(type: "int", nullable: true),
                    OwnGoalsScoredCount = table.Column<int>(type: "int", nullable: true),
                    TotalRequests = table.Column<int>(type: "int", nullable: true),
                    SucceededRequests = table.Column<int>(type: "int", nullable: true),
                    FailedRequests = table.Column<int>(type: "int", nullable: true),
                    SuccessRate = table.Column<double>(type: "float", nullable: true),
                    FailureRate = table.Column<double>(type: "float", nullable: true),
                    TotalTransactionsCount = table.Column<int>(type: "int", nullable: true),
                    SuccessfulPaymentsCount = table.Column<int>(type: "int", nullable: true),
                    SuccessfulPaymentsRate = table.Column<double>(type: "float", nullable: true),
                    UnsuccessfulPaymentsCount = table.Column<int>(type: "int", nullable: true),
                    UnsuccessfulPaymentsRate = table.Column<double>(type: "float", nullable: true),
                    TransfersReports_LeagueId = table.Column<int>(type: "int", nullable: true),
                    TransferPeriodId = table.Column<int>(type: "int", nullable: true),
                    TransferMarketCount = table.Column<int>(type: "int", nullable: true),
                    PurchasedPlayersCount = table.Column<int>(type: "int", nullable: true),
                    DeclinedTransfersCount = table.Column<int>(type: "int", nullable: true),
                    TranferAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssociationCut = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClubsCut = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuccessfulTranferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnsuccessfulTranferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotStartedTransferRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalUserAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveUserAccountsCount = table.Column<int>(type: "int", nullable: true),
                    ActiveUserAccountsRate = table.Column<double>(type: "float", nullable: true),
                    InactiveUserAccountsCount = table.Column<int>(type: "int", nullable: true),
                    InactiveUserAccountsRate = table.Column<double>(type: "float", nullable: true),
                    SuspendedUserAccountsCount = table.Column<int>(type: "int", nullable: true),
                    SuspendedUserAccountsRate = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_Club_ClubPerformanceReport_ClubId",
                        column: x => x.ClubPerformanceReport_ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Reports_League_ClubPerformanceReport_LeagueId",
                        column: x => x.ClubPerformanceReport_LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_League_ClubTransferReport_LeagueId",
                        column: x => x.ClubTransferReport_LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_League_MatchReports_LeagueId",
                        column: x => x.MatchReports_LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_League_MatchResultsReports_LeagueId",
                        column: x => x.MatchResultsReports_LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_League_TransfersReports_LeagueId",
                        column: x => x.TransfersReports_LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_SportNew_SportNewsId",
                        column: x => x.SportNewsId,
                        principalTable: "SportNew",
                        principalColumn: "NewsId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reports_TransferPeriod_TransferPeriodId",
                        column: x => x.TransferPeriodId,
                        principalTable: "TransferPeriod",
                        principalColumn: "TransferPeriodId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LineUp",
                columns: table => new
                {
                    LineUpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUp", x => x.LineUpId);
                    table.ForeignKey(
                        name: "FK_LineUp_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUp_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUp_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUp_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LineUpSubstitutesHolder",
                columns: table => new
                {
                    LineUpSubstituteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUpSubstitutesHolder", x => x.LineUpSubstituteId);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutesHolder_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutesHolder_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutesHolder_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutesHolder_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutesHolder_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LineUpXIHolder",
                columns: table => new
                {
                    LineUpXIId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUpXIHolder", x => x.LineUpXIId);
                    table.ForeignKey(
                        name: "FK_LineUpXIHolder_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXIHolder_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXIHolder_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXIHolder_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXIHolder_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Live",
                columns: table => new
                {
                    LiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    LiveTime = table.Column<int>(type: "int", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "int", nullable: false),
                    AwayTeamScore = table.Column<int>(type: "int", nullable: false),
                    IsLive = table.Column<bool>(type: "bit", nullable: false),
                    IsHalfTime = table.Column<bool>(type: "bit", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    ISEnded = table.Column<bool>(type: "bit", nullable: false),
                    IsInterrupted = table.Column<bool>(type: "bit", nullable: false),
                    WentToHalfTime = table.Column<bool>(type: "bit", nullable: false),
                    AddedTime = table.Column<int>(type: "int", nullable: true),
                    HalfTimeScore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LiveStatus = table.Column<int>(type: "int", nullable: false),
                    ReasonForInterruption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Live", x => x.LiveId);
                    table.ForeignKey(
                        name: "FK_Live_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Live_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Live_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "MatchFormation",
                columns: table => new
                {
                    MatchFormationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    FormationId = table.Column<int>(type: "int", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchFormation", x => x.MatchFormationId);
                    table.ForeignKey(
                        name: "FK_MatchFormation_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchFormation_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchFormation_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchFormation_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchFormation_Formations_FormationId",
                        column: x => x.FormationId,
                        principalTable: "Formations",
                        principalColumn: "FormationId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "MatchOfficials",
                columns: table => new
                {
                    MatchOfficialsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    RefeereId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssistantOneId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssistantTwoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchOfficials", x => x.MatchOfficialsId);
                    table.ForeignKey(
                        name: "FK_MatchOfficials_AspNetUsers_AssistantOneId",
                        column: x => x.AssistantOneId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchOfficials_AspNetUsers_AssistantTwoId",
                        column: x => x.AssistantTwoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchOfficials_AspNetUsers_RefeereId",
                        column: x => x.RefeereId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchOfficials_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_MatchOfficials_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "MatchResult",
                columns: table => new
                {
                    ResultsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "int", nullable: false),
                    AwayTeamScore = table.Column<int>(type: "int", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResult", x => x.ResultsId);
                    table.ForeignKey(
                        name: "FK_MatchResult_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchResult_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchResult_Club_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchResult_Club_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchResult_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_MatchResult_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MatchResult_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Transfer",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerClubId = table.Column<int>(type: "int", nullable: false),
                    PlayerTransferMarketId = table.Column<int>(type: "int", nullable: false),
                    SellerClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Approved_Declined_ById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    paymentTransferStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_Transfer_AspNetUsers_Approved_Declined_ById",
                        column: x => x.Approved_Declined_ById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_Club_CustomerClubId",
                        column: x => x.CustomerClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_Club_SellerClubId",
                        column: x => x.SellerClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_Transfer_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transfer_PlayerTransferMarket_PlayerTransferMarketId",
                        column: x => x.PlayerTransferMarketId,
                        principalTable: "PlayerTransferMarket",
                        principalColumn: "PlayerTransferMarketId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionHistories",
                columns: table => new
                {
                    MySubscriptionHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    Plan = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubscribedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHistories", x => x.MySubscriptionHistoryId);
                    table.ForeignKey(
                        name: "FK_SubscriptionHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionHistories_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId");
                    table.ForeignKey(
                        name: "FK_SubscriptionHistories_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LineUpSubstitutes",
                columns: table => new
                {
                    LineUpSubstituteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    LineUpId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUpSubstitutes", x => x.LineUpSubstituteId);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpSubstitutes_LineUp_LineUpId",
                        column: x => x.LineUpId,
                        principalTable: "LineUp",
                        principalColumn: "LineUpId");
                });

            migrationBuilder.CreateTable(
                name: "LineUpXI",
                columns: table => new
                {
                    LineUpXIId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    LineUpId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUpXI", x => x.LineUpXIId);
                    table.ForeignKey(
                        name: "FK_LineUpXI_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXI_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXI_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXI_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXI_Fixture_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixture",
                        principalColumn: "FixtureId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LineUpXI_LineUp_LineUpId",
                        column: x => x.LineUpId,
                        principalTable: "LineUp",
                        principalColumn: "LineUpId");
                });

            migrationBuilder.CreateTable(
                name: "LiveEvents",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveId = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    RecordedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    ScoreById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScoredTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssistedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScoredById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LiveGoalHolder_ScoredTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiveGoalHolder_AssistedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OwnGoalScoredById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OwnGoalTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedCommitedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RedCardTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedCardReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YellowCommitedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    YellowCardTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YellowCardReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PenaltyTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RedCard_RedCommitedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RedCardTime1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedCardReason1 = table.Column<int>(type: "int", nullable: true),
                    OutPlayerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InPlayerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YellowCard_YellowCommitedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    YellowCardTime1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YellowCardReason1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_AssistedById",
                        column: x => x.AssistedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_InPlayerId",
                        column: x => x.InPlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_LiveGoalHolder_AssistedById",
                        column: x => x.LiveGoalHolder_AssistedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_OutPlayerId",
                        column: x => x.OutPlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_OwnGoalScoredById",
                        column: x => x.OwnGoalScoredById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_RedCard_RedCommitedById",
                        column: x => x.RedCard_RedCommitedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_RedCommitedById",
                        column: x => x.RedCommitedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_ScoreById",
                        column: x => x.ScoreById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_ScoredById",
                        column: x => x.ScoredById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_YellowCard_YellowCommitedById",
                        column: x => x.YellowCard_YellowCommitedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_AspNetUsers_YellowCommitedById",
                        column: x => x.YellowCommitedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "DivisionId");
                    table.ForeignKey(
                        name: "FK_LiveEvents_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LiveEvents_Live_LiveId",
                        column: x => x.LiveId,
                        principalTable: "Live",
                        principalColumn: "LiveId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    FineId = table.Column<int>(type: "int", nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    InvoiceTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEmailed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Invoices_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "FineId");
                    table.ForeignKey(
                        name: "FK_Invoices_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_Invoices_Transfer_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfer",
                        principalColumn: "TransferId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_DeviceInfoId",
                table: "ActivityLogs",
                column: "DeviceInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserBaseModelId",
                table: "ActivityLogs",
                column: "UserBaseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedById",
                table: "Announcements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_DivisionId",
                table: "Announcements",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ModifiedById",
                table: "Announcements",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClubId",
                table: "AspNetUsers",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClubManager_ClubId",
                table: "AspNetUsers",
                column: "ClubManager_ClubId",
                unique: true,
                filter: "[ClubId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClubManager_DivisionId",
                table: "AspNetUsers",
                column: "ClubManager_DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DivisionId",
                table: "AspNetUsers",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DivisionManager_DivisionId",
                table: "AspNetUsers",
                column: "DivisionManager_DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Officials_DivisionId",
                table: "AspNetUsers",
                column: "Officials_DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Player_ClubId",
                table: "AspNetUsers",
                column: "Player_ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Player_DivisionId",
                table: "AspNetUsers",
                column: "Player_DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SportsMember_DivisionId",
                table: "AspNetUsers",
                column: "SportsMember_DivisionId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Club_CreatedById",
                table: "Club",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Club_DivisionId",
                table: "Club",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Club_LeagueId",
                table: "Club",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Club_ModifiedById",
                table: "Club",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClubWarnings_CLubId",
                table: "ClubWarnings",
                column: "CLubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubWarnings_CreatedById",
                table: "ClubWarnings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClubWarnings_ModifiedById",
                table: "ClubWarnings",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommentById",
                table: "Comments",
                column: "CommentById");

            migrationBuilder.CreateIndex(
                name: "IX_Competition_WinnerId",
                table: "Competition",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionParticipants_CompetitionId",
                table: "CompetitionParticipants",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionParticipants_UserId",
                table: "CompetitionParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionAggreements_CreatedById",
                table: "DivisionAggreements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionAggreements_DivisionId",
                table: "DivisionAggreements",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionAggreements_ModifiedById",
                table: "DivisionAggreements",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_CreatedById",
                table: "Divisions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_ModifiedById",
                table: "Divisions",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_ClubId",
                table: "Fines",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_CreatedById",
                table: "Fines",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_DivisionId",
                table: "Fines",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_ModifiedById",
                table: "Fines",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_OffenderId",
                table: "Fines",
                column: "OffenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_AwayTeamId",
                table: "Fixture",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_CreatedById",
                table: "Fixture",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_DivisionId",
                table: "Fixture",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_HomeTeamId",
                table: "Fixture",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_KickOffDate",
                table: "Fixture",
                column: "KickOffDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_LeagueId",
                table: "Fixture",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixture_ModifiedById",
                table: "Fixture",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Formations_CreatedById",
                table: "Formations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Formations_ModifiedById",
                table: "Formations",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_HeadToHead_AwayTeamId",
                table: "HeadToHead",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HeadToHead_ClubId",
                table: "HeadToHead",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_HeadToHead_HomeTeamId",
                table: "HeadToHead",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedById",
                table: "Invoices",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_FineId",
                table: "Invoices",
                column: "FineId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentId",
                table: "Invoices",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TransferId",
                table: "Invoices",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_League_CreatedById",
                table: "League",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_League_DivisionId",
                table: "League",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_League_ModifiedById",
                table: "League",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUp_ClubId",
                table: "LineUp",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUp_CreatedById",
                table: "LineUp",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUp_FixtureId",
                table: "LineUp",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUp_ModifiedById",
                table: "LineUp",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_ClubId",
                table: "LineUpSubstitutes",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_CreatedById",
                table: "LineUpSubstitutes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_FixtureId",
                table: "LineUpSubstitutes",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_LineUpId",
                table: "LineUpSubstitutes",
                column: "LineUpId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_ModifiedById",
                table: "LineUpSubstitutes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutes_PlayerId",
                table: "LineUpSubstitutes",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutesHolder_ClubId",
                table: "LineUpSubstitutesHolder",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutesHolder_CreatedById",
                table: "LineUpSubstitutesHolder",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutesHolder_FixtureId",
                table: "LineUpSubstitutesHolder",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutesHolder_ModifiedById",
                table: "LineUpSubstitutesHolder",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpSubstitutesHolder_PlayerId",
                table: "LineUpSubstitutesHolder",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_ClubId",
                table: "LineUpXI",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_CreatedById",
                table: "LineUpXI",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_FixtureId",
                table: "LineUpXI",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_LineUpId",
                table: "LineUpXI",
                column: "LineUpId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_ModifiedById",
                table: "LineUpXI",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXI_PlayerId",
                table: "LineUpXI",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXIHolder_ClubId",
                table: "LineUpXIHolder",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXIHolder_CreatedById",
                table: "LineUpXIHolder",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXIHolder_FixtureId",
                table: "LineUpXIHolder",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXIHolder_ModifiedById",
                table: "LineUpXIHolder",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_LineUpXIHolder_PlayerId",
                table: "LineUpXIHolder",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Live_DivisionId",
                table: "Live",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Live_FixtureId",
                table: "Live",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_Live_LeagueId",
                table: "Live",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_AssistedById",
                table: "LiveEvents",
                column: "AssistedById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_DivisionId",
                table: "LiveEvents",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_InPlayerId",
                table: "LiveEvents",
                column: "InPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_LeagueId",
                table: "LiveEvents",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_LiveGoalHolder_AssistedById",
                table: "LiveEvents",
                column: "LiveGoalHolder_AssistedById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_LiveId",
                table: "LiveEvents",
                column: "LiveId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_OutPlayerId",
                table: "LiveEvents",
                column: "OutPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_OwnGoalScoredById",
                table: "LiveEvents",
                column: "OwnGoalScoredById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_PlayerId",
                table: "LiveEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_RedCard_RedCommitedById",
                table: "LiveEvents",
                column: "RedCard_RedCommitedById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_RedCommitedById",
                table: "LiveEvents",
                column: "RedCommitedById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_ScoreById",
                table: "LiveEvents",
                column: "ScoreById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_ScoredById",
                table: "LiveEvents",
                column: "ScoredById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_YellowCard_YellowCommitedById",
                table: "LiveEvents",
                column: "YellowCard_YellowCommitedById");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_YellowCommitedById",
                table: "LiveEvents",
                column: "YellowCommitedById");

            migrationBuilder.CreateIndex(
                name: "IX_Maintainances_CreatedById",
                table: "Maintainances",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Maintainances_ResolvedById",
                table: "Maintainances",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormation_ClubId",
                table: "MatchFormation",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormation_CreatedById",
                table: "MatchFormation",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormation_FixtureId",
                table: "MatchFormation",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormation_FormationId",
                table: "MatchFormation",
                column: "FormationId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormation_ModifiedById",
                table: "MatchFormation",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MatchOfficials_AssistantOneId",
                table: "MatchOfficials",
                column: "AssistantOneId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchOfficials_AssistantTwoId",
                table: "MatchOfficials",
                column: "AssistantTwoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchOfficials_DivisionId",
                table: "MatchOfficials",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchOfficials_FixtureId",
                table: "MatchOfficials",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchOfficials_RefeereId",
                table: "MatchOfficials",
                column: "RefeereId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_AwayTeamId",
                table: "MatchResult",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_CreatedById",
                table: "MatchResult",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_DivisionId",
                table: "MatchResult",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_FixtureId",
                table: "MatchResult",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_HomeTeamId",
                table: "MatchResult",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_LeagueId",
                table: "MatchResult",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_ModifiedById",
                table: "MatchResult",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_CreatedById",
                table: "Meeting",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_DivisionId",
                table: "Meeting",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_ModifiedById",
                table: "Meeting",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClubId",
                table: "Payments",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DeviceInfoId",
                table: "Payments",
                column: "DeviceInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DivisionId",
                table: "Payments",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMadeById",
                table: "Payments",
                column: "PaymentMadeById");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransferMarket_ClubId",
                table: "PlayerTransferMarket",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransferMarket_CreatedById",
                table: "PlayerTransferMarket",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransferMarket_DivisionId",
                table: "PlayerTransferMarket",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransferMarket_LeagueId",
                table: "PlayerTransferMarket",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransferMarket_PlayerId",
                table: "PlayerTransferMarket",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClubId",
                table: "Reports",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClubPerformanceReport_ClubId",
                table: "Reports",
                column: "ClubPerformanceReport_ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClubPerformanceReport_LeagueId",
                table: "Reports",
                column: "ClubPerformanceReport_LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClubTransferReport_LeagueId",
                table: "Reports",
                column: "ClubTransferReport_LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DivisionId",
                table: "Reports",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_LeagueId",
                table: "Reports",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MatchReports_LeagueId",
                table: "Reports",
                column: "MatchReports_LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MatchResultsReports_LeagueId",
                table: "Reports",
                column: "MatchResultsReports_LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PlayerId",
                table: "Reports",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SportNewsId",
                table: "Reports",
                column: "SportNewsId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TransferPeriodId",
                table: "Reports",
                column: "TransferPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TransfersReports_LeagueId",
                table: "Reports",
                column: "TransfersReports_LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_SportNew_AuthoredById",
                table: "SportNew",
                column: "AuthoredById");

            migrationBuilder.CreateIndex(
                name: "IX_SportNew_DivisionId",
                table: "SportNew",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SportNew_ModifiedById",
                table: "SportNew",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_SportNew_PublishedById",
                table: "SportNew",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_SportNew_RejectedById",
                table: "SportNew",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_Standing_ClubId",
                table: "Standing",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Standing_CreatedById",
                table: "Standing",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Standing_DivisionId",
                table: "Standing",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Standing_LeagueId",
                table: "Standing",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Standing_ModifiedById",
                table: "Standing",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_ClubId",
                table: "SubscriptionHistories",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_SubscriptionId",
                table: "SubscriptionHistories",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_UserId",
                table: "SubscriptionHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClubId",
                table: "Subscriptions",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TermsAggreements_UserBaseModelId",
                table: "TermsAggreements",
                column: "UserBaseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TopAssists_DivisionId",
                table: "TopAssists",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TopAssists_LeagueId",
                table: "TopAssists",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_TopAssists_PlayerId",
                table: "TopAssists",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_DivisionId",
                table: "TopScores",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_LeagueId",
                table: "TopScores",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_PlayerId",
                table: "TopScores",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_CreatedById",
                table: "Tournament",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_ModifiedById",
                table: "Tournament",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_Approved_Declined_ById",
                table: "Transfer",
                column: "Approved_Declined_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_CreatedById",
                table: "Transfer",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_CustomerClubId",
                table: "Transfer",
                column: "CustomerClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_DivisionId",
                table: "Transfer",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_LeagueId",
                table: "Transfer",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_ModifiedById",
                table: "Transfer",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_PlayerId",
                table: "Transfer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_PlayerTransferMarketId",
                table: "Transfer",
                column: "PlayerTransferMarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_SellerClubId",
                table: "Transfer",
                column: "SellerClubId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferPeriod_CreatedById",
                table: "TransferPeriod",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TransferPeriod_DivisionId",
                table: "TransferPeriod",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferPeriod_LeagueId",
                table: "TransferPeriod",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferPeriod_ModifiedById",
                table: "TransferPeriod",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_CreatedById",
                table: "Warnings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_ModifiedById",
                table: "Warnings",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_UserBaseModelId",
                table: "Warnings",
                column: "UserBaseModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_AspNetUsers_UserBaseModelId",
                table: "ActivityLogs",
                column: "UserBaseModelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_AspNetUsers_CreatedById",
                table: "Announcements",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_AspNetUsers_ModifiedById",
                table: "Announcements",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Divisions_DivisionId",
                table: "Announcements",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Club_ClubId",
                table: "AspNetUsers",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Club_ClubManager_ClubId",
                table: "AspNetUsers",
                column: "ClubManager_ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Club_Player_ClubId",
                table: "AspNetUsers",
                column: "Player_ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_ClubManager_DivisionId",
                table: "AspNetUsers",
                column: "ClubManager_DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_DivisionId",
                table: "AspNetUsers",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_DivisionManager_DivisionId",
                table: "AspNetUsers",
                column: "DivisionManager_DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_Officials_DivisionId",
                table: "AspNetUsers",
                column: "Officials_DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_Player_DivisionId",
                table: "AspNetUsers",
                column: "Player_DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Divisions_SportsMember_DivisionId",
                table: "AspNetUsers",
                column: "SportsMember_DivisionId",
                principalTable: "Divisions",
                principalColumn: "DivisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Club_AspNetUsers_CreatedById",
                table: "Club");

            migrationBuilder.DropForeignKey(
                name: "FK_Club_AspNetUsers_ModifiedById",
                table: "Club");

            migrationBuilder.DropForeignKey(
                name: "FK_Divisions_AspNetUsers_CreatedById",
                table: "Divisions");

            migrationBuilder.DropForeignKey(
                name: "FK_Divisions_AspNetUsers_ModifiedById",
                table: "Divisions");

            migrationBuilder.DropForeignKey(
                name: "FK_League_AspNetUsers_CreatedById",
                table: "League");

            migrationBuilder.DropForeignKey(
                name: "FK_League_AspNetUsers_ModifiedById",
                table: "League");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ClubWarnings");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CompetitionParticipants");

            migrationBuilder.DropTable(
                name: "CookiePreferences");

            migrationBuilder.DropTable(
                name: "DivisionAggreements");

            migrationBuilder.DropTable(
                name: "HeadToHead");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LineUpSubstitutes");

            migrationBuilder.DropTable(
                name: "LineUpSubstitutesHolder");

            migrationBuilder.DropTable(
                name: "LineUpXI");

            migrationBuilder.DropTable(
                name: "LineUpXIHolder");

            migrationBuilder.DropTable(
                name: "LiveEvents");

            migrationBuilder.DropTable(
                name: "Maintainances");

            migrationBuilder.DropTable(
                name: "MatchFormation");

            migrationBuilder.DropTable(
                name: "MatchOfficials");

            migrationBuilder.DropTable(
                name: "MatchResult");

            migrationBuilder.DropTable(
                name: "Meeting");

            migrationBuilder.DropTable(
                name: "OnboardingRequests");

            migrationBuilder.DropTable(
                name: "PayFastWebhookRecords");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "RequestLogs");

            migrationBuilder.DropTable(
                name: "Standing");

            migrationBuilder.DropTable(
                name: "SubscriptionHistories");

            migrationBuilder.DropTable(
                name: "TermsAggreements");

            migrationBuilder.DropTable(
                name: "TestUserFeedbacks");

            migrationBuilder.DropTable(
                name: "TopAssists");

            migrationBuilder.DropTable(
                name: "TopScores");

            migrationBuilder.DropTable(
                name: "Tournament");

            migrationBuilder.DropTable(
                name: "Warnings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Competition");

            migrationBuilder.DropTable(
                name: "Fines");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Transfer");

            migrationBuilder.DropTable(
                name: "LineUp");

            migrationBuilder.DropTable(
                name: "Live");

            migrationBuilder.DropTable(
                name: "Formations");

            migrationBuilder.DropTable(
                name: "SportNew");

            migrationBuilder.DropTable(
                name: "TransferPeriod");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "DeviceInfo");

            migrationBuilder.DropTable(
                name: "PlayerTransferMarket");

            migrationBuilder.DropTable(
                name: "Fixture");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Club");

            migrationBuilder.DropTable(
                name: "League");

            migrationBuilder.DropTable(
                name: "Divisions");
        }
    }
}
