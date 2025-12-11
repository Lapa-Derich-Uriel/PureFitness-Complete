using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureFitness.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Equipment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Equipment_Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Equipment_Category = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    Program = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Equipmen__C0F077C54A02FF36", x => x.Equipment_ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Role_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role_name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__D80AB49B36427027", x => x.Role_ID);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Supplier_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Supplier__83918DB87377D50D", x => x.Supplier_Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentItem",
                columns: table => new
                {
                    Equipment_ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Equipment_ItemName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Equipment_ItemStatus = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Equipment_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Equipmen__4428EEFA2448AA05", x => x.Equipment_ItemID);
                    table.ForeignKey(
                        name: "FK__Equipment__Equip__44FF419A",
                        column: x => x.Equipment_ID,
                        principalTable: "Equipments",
                        principalColumn: "Equipment_ID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__206D9190BAC38406", x => x.User_ID);
                    table.ForeignKey(
                        name: "FK__Users__Role_ID__398D8EEE",
                        column: x => x.Role_ID,
                        principalTable: "Roles",
                        principalColumn: "Role_ID");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Product_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Supplier_Id = table.Column<int>(type: "int", nullable: true),
                    Product_Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Product_Price = table.Column<decimal>(type: "decimal(18,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Products__9834FBBACB251D5E", x => x.Product_Id);
                    table.ForeignKey(
                        name: "FK__Products__Suppli__5165187F",
                        column: x => x.Supplier_Id,
                        principalTable: "Suppliers",
                        principalColumn: "Supplier_Id");
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Staff_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Phone_Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    User_ID = table.Column<int>(type: "int", nullable: true),
                    AssignedTask = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Staffs__32D1F3C35688645F", x => x.Staff_ID);
                    table.ForeignKey(
                        name: "FK__Staffs__User_ID__3C69FB99",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID");
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Inventory_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Id = table.Column<int>(type: "int", nullable: true),
                    Product_Quantity = table.Column<int>(type: "int", nullable: true),
                    Product_Status = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__2B65F40B43D6D272", x => x.Inventory_ID);
                    table.ForeignKey(
                        name: "FK__Inventory__Produ__5441852A",
                        column: x => x.Product_Id,
                        principalTable: "Products",
                        principalColumn: "Product_Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Equipment_ID = table.Column<int>(type: "int", nullable: true),
                    Equipment_ItemID = table.Column<int>(type: "int", nullable: true),
                    MaintenanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Staff_ID = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Frequency = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    NextMaintenanceDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Equipmen__9C8A5B49BA06112B", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK__Equipment__Equip__47DBAE45",
                        column: x => x.Equipment_ID,
                        principalTable: "Equipments",
                        principalColumn: "Equipment_ID");
                    table.ForeignKey(
                        name: "FK__Equipment__Equip__48CFD27E",
                        column: x => x.Equipment_ItemID,
                        principalTable: "EquipmentItem",
                        principalColumn: "Equipment_ItemID");
                    table.ForeignKey(
                        name: "FK__Equipment__Staff__49C3F6B7",
                        column: x => x.Staff_ID,
                        principalTable: "Staffs",
                        principalColumn: "Staff_ID");
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Member_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    LName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Phone_Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Membership_Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Access_Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Membership_Period = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Fitness_Goal = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    BMI = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Date_Joined = table.Column<DateOnly>(type: "date", nullable: true),
                    Due_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    User_ID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Paid_Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Staff_ID = table.Column<int>(type: "int", nullable: true),
                    Cancellation_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    ActivityPoints = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Members__42A68F2704A2A553", x => x.Member_ID);
                    table.ForeignKey(
                        name: "FK__Members__Staff_I__403A8C7D",
                        column: x => x.Staff_ID,
                        principalTable: "Staffs",
                        principalColumn: "Staff_ID");
                    table.ForeignKey(
                        name: "FK__Members__User_ID__3F466844",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID");
                });

            migrationBuilder.CreateTable(
                name: "Equipment_Logs",
                columns: table => new
                {
                    Log_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Equipmen__2D26E7AED89A69D3", x => x.Log_ID);
                    table.ForeignKey(
                        name: "FK__Equipment__Sched__4CA06362",
                        column: x => x.ScheduleId,
                        principalTable: "EquipmentSchedules",
                        principalColumn: "ScheduleId");
                });

            migrationBuilder.CreateTable(
                name: "DietPlan",
                columns: table => new
                {
                    DietPlan_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Member_ID = table.Column<int>(type: "int", nullable: true),
                    Staff_ID = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DietPlan__92355CDC9ECA58FD", x => x.DietPlan_ID);
                    table.ForeignKey(
                        name: "FK__DietPlan__Member__68487DD7",
                        column: x => x.Member_ID,
                        principalTable: "Members",
                        principalColumn: "Member_ID");
                    table.ForeignKey(
                        name: "FK__DietPlan__Staff___693CA210",
                        column: x => x.Staff_ID,
                        principalTable: "Staffs",
                        principalColumn: "Staff_ID");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Receipt_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Staff_ID = table.Column<int>(type: "int", nullable: true),
                    Member_ID = table.Column<int>(type: "int", nullable: true),
                    Transaction_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Payment_Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Inventory_ID = table.Column<int>(type: "int", nullable: true),
                    Items = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    AccessType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Total_Cost = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Change = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    WalkInName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ItemQuantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__38B787CCCBC02627", x => x.Receipt_ID);
                    table.ForeignKey(
                        name: "FK__Payments__Invent__59063A47",
                        column: x => x.Inventory_ID,
                        principalTable: "Inventory",
                        principalColumn: "Inventory_ID");
                    table.ForeignKey(
                        name: "FK__Payments__Member__5812160E",
                        column: x => x.Member_ID,
                        principalTable: "Members",
                        principalColumn: "Member_ID");
                    table.ForeignKey(
                        name: "FK__Payments__Staff___571DF1D5",
                        column: x => x.Staff_ID,
                        principalTable: "Staffs",
                        principalColumn: "Staff_ID");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlan",
                columns: table => new
                {
                    WorkoutPlan_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Member_ID = table.Column<int>(type: "int", nullable: true),
                    Staff_ID = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkoutP__D9FFB67298176932", x => x.WorkoutPlan_ID);
                    table.ForeignKey(
                        name: "FK__WorkoutPl__Membe__5FB337D6",
                        column: x => x.Member_ID,
                        principalTable: "Members",
                        principalColumn: "Member_ID");
                    table.ForeignKey(
                        name: "FK__WorkoutPl__Staff__60A75C0F",
                        column: x => x.Staff_ID,
                        principalTable: "Staffs",
                        principalColumn: "Staff_ID");
                });

            migrationBuilder.CreateTable(
                name: "DietPlanItem",
                columns: table => new
                {
                    DietPlanItem_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DietPlan_ID = table.Column<int>(type: "int", nullable: true),
                    CalorieTarget = table.Column<int>(type: "int", nullable: false),
                    MealName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DietPlan__DBEA1A9CE779835C", x => x.DietPlanItem_ID);
                    table.ForeignKey(
                        name: "FK__DietPlanI__DietP__6D0D32F4",
                        column: x => x.DietPlan_ID,
                        principalTable: "DietPlan",
                        principalColumn: "DietPlan_ID");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Attendance_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Member_ID = table.Column<int>(type: "int", nullable: true),
                    Attendance_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    TimeIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimeOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    Attendance_Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Activity = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Receipt_ID = table.Column<int>(type: "int", nullable: true),
                    TimeSpent = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attendan__57FA4934CD4969B3", x => x.Attendance_ID);
                    table.ForeignKey(
                        name: "FK__Attendanc__Membe__5BE2A6F2",
                        column: x => x.Member_ID,
                        principalTable: "Members",
                        principalColumn: "Member_ID");
                    table.ForeignKey(
                        name: "FK__Attendanc__Recei__5CD6CB2B",
                        column: x => x.Receipt_ID,
                        principalTable: "Payments",
                        principalColumn: "Receipt_ID");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlanItem",
                columns: table => new
                {
                    Workout_Plan_ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutPlan_ID = table.Column<int>(type: "int", nullable: true),
                    Equipment_ID = table.Column<int>(type: "int", nullable: true),
                    Exercise_Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: false),
                    Repetitions = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkoutP__1B6AA810E2480470", x => x.Workout_Plan_ItemID);
                    table.ForeignKey(
                        name: "FK__WorkoutPl__Equip__656C112C",
                        column: x => x.Equipment_ID,
                        principalTable: "Equipments",
                        principalColumn: "Equipment_ID");
                    table.ForeignKey(
                        name: "FK__WorkoutPl__Worko__6477ECF3",
                        column: x => x.WorkoutPlan_ID,
                        principalTable: "WorkoutPlan",
                        principalColumn: "WorkoutPlan_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Member_ID",
                table: "Attendances",
                column: "Member_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Receipt_ID",
                table: "Attendances",
                column: "Receipt_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlan_Member_ID",
                table: "DietPlan",
                column: "Member_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlan_Staff_ID",
                table: "DietPlan",
                column: "Staff_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlanItem_DietPlan_ID",
                table: "DietPlanItem",
                column: "DietPlan_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Logs_ScheduleId",
                table: "Equipment_Logs",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItem_Equipment_ID",
                table: "EquipmentItem",
                column: "Equipment_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSchedules_Equipment_ID",
                table: "EquipmentSchedules",
                column: "Equipment_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSchedules_Equipment_ItemID",
                table: "EquipmentSchedules",
                column: "Equipment_ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSchedules_Staff_ID",
                table: "EquipmentSchedules",
                column: "Staff_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Product_Id",
                table: "Inventory",
                column: "Product_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Staff_ID",
                table: "Members",
                column: "Staff_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_User_ID",
                table: "Members",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Inventory_ID",
                table: "Payments",
                column: "Inventory_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Member_ID",
                table: "Payments",
                column: "Member_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Staff_ID",
                table: "Payments",
                column: "Staff_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Supplier_Id",
                table: "Products",
                column: "Supplier_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_User_ID",
                table: "Staffs",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_ID",
                table: "Users",
                column: "Role_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlan_Member_ID",
                table: "WorkoutPlan",
                column: "Member_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlan_Staff_ID",
                table: "WorkoutPlan",
                column: "Staff_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanItem_Equipment_ID",
                table: "WorkoutPlanItem",
                column: "Equipment_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanItem_WorkoutPlan_ID",
                table: "WorkoutPlanItem",
                column: "WorkoutPlan_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "DietPlanItem");

            migrationBuilder.DropTable(
                name: "Equipment_Logs");

            migrationBuilder.DropTable(
                name: "WorkoutPlanItem");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "DietPlan");

            migrationBuilder.DropTable(
                name: "EquipmentSchedules");

            migrationBuilder.DropTable(
                name: "WorkoutPlan");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "EquipmentItem");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
