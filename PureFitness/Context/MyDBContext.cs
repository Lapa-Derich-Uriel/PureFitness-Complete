using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PureFitness.Models;

namespace PureFitness.Context;

public partial class MyDBContext : DbContext
{
    public MyDBContext()
    {
    }

    public MyDBContext(DbContextOptions<MyDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<DietPlan> DietPlans { get; set; }

    public virtual DbSet<DietPlanItem> DietPlanItems { get; set; }

    public virtual DbSet<Equipment> Equipments { get; set; }

    public virtual DbSet<EquipmentItem> EquipmentItems { get; set; }

    public virtual DbSet<EquipmentLog> EquipmentLogs { get; set; }

    public virtual DbSet<EquipmentSchedule> EquipmentSchedules { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkoutPlan> WorkoutPlans { get; set; }

    public virtual DbSet<WorkoutPlanItem> WorkoutPlanItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-TP15H8QU\\MSSQLSERVER01;Initial Catalog=PureFitness;Integrated Security=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__57FA4934CD4969B3");

            entity.Property(e => e.AttendanceId).HasColumnName("Attendance_ID");
            entity.Property(e => e.Activity)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.AttendanceDate).HasColumnName("Attendance_Date");
            entity.Property(e => e.AttendanceStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Attendance_Status");
            entity.Property(e => e.MemberId).HasColumnName("Member_ID");
            entity.Property(e => e.ReceiptId).HasColumnName("Receipt_ID");

            entity.HasOne(d => d.Member).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__Attendanc__Membe__5BE2A6F2");

            entity.HasOne(d => d.Receipt).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("FK__Attendanc__Recei__5CD6CB2B");
        });

        modelBuilder.Entity<DietPlan>(entity =>
        {
            entity.HasKey(e => e.DietPlanId).HasName("PK__DietPlan__92355CDC9ECA58FD");

            entity.ToTable("DietPlan");

            entity.Property(e => e.DietPlanId).HasColumnName("DietPlan_ID");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasColumnName("Member_ID");
            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");

            entity.HasOne(d => d.Member).WithMany(p => p.DietPlans)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__DietPlan__Member__68487DD7");

            entity.HasOne(d => d.Staff).WithMany(p => p.DietPlans)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__DietPlan__Staff___693CA210");
        });

        modelBuilder.Entity<DietPlanItem>(entity =>
        {
            entity.HasKey(e => e.DietPlanItemId).HasName("PK__DietPlan__DBEA1A9CE779835C");

            entity.ToTable("DietPlanItem");

            entity.Property(e => e.DietPlanItemId).HasColumnName("DietPlanItem_ID");
            entity.Property(e => e.DietPlanId).HasColumnName("DietPlan_ID");
            entity.Property(e => e.MealName).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.DietPlan).WithMany(p => p.DietPlanItems)
                .HasForeignKey(d => d.DietPlanId)
                .HasConstraintName("FK__DietPlanI__DietP__6D0D32F4");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__C0F077C54A02FF36");

            entity.Property(e => e.EquipmentId).HasColumnName("Equipment_ID");
            entity.Property(e => e.EquipmentCategory)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Equipment_Category");
            entity.Property(e => e.EquipmentName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Equipment_Name");
            entity.Property(e => e.Program)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EquipmentItem>(entity =>
        {
            entity.HasKey(e => e.EquipmentItemId).HasName("PK__Equipmen__4428EEFA2448AA05");

            entity.ToTable("EquipmentItem");

            entity.Property(e => e.EquipmentItemId).HasColumnName("Equipment_ItemID");
            entity.Property(e => e.EquipmentId).HasColumnName("Equipment_ID");
            entity.Property(e => e.EquipmentItemName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Equipment_ItemName");
            entity.Property(e => e.EquipmentItemStatus)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Equipment_ItemStatus");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EquipmentItems)
                .HasForeignKey(d => d.EquipmentId)
                .HasConstraintName("FK__Equipment__Equip__44FF419A");
        });

        modelBuilder.Entity<EquipmentLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Equipmen__2D26E7AED89A69D3");

            entity.ToTable("Equipment_Logs");

            entity.Property(e => e.LogId).HasColumnName("Log_ID");

            entity.HasOne(d => d.Schedule).WithMany(p => p.EquipmentLogs)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("FK__Equipment__Sched__4CA06362");
        });

        modelBuilder.Entity<EquipmentSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Equipmen__9C8A5B49BA06112B");

            entity.Property(e => e.EquipmentId).HasColumnName("Equipment_ID");
            entity.Property(e => e.EquipmentItemId).HasColumnName("Equipment_ItemID");
            entity.Property(e => e.Frequency)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EquipmentSchedules)
                .HasForeignKey(d => d.EquipmentId)
                .HasConstraintName("FK__Equipment__Equip__47DBAE45");

            entity.HasOne(d => d.EquipmentItem).WithMany(p => p.EquipmentSchedules)
                .HasForeignKey(d => d.EquipmentItemId)
                .HasConstraintName("FK__Equipment__Equip__48CFD27E");

            entity.HasOne(d => d.Staff).WithMany(p => p.EquipmentSchedules)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Equipment__Staff__49C3F6B7");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__2B65F40B43D6D272");

            entity.ToTable("Inventory");

            entity.Property(e => e.InventoryId).HasColumnName("Inventory_ID");
            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.ProductQuantity).HasColumnName("Product_Quantity");
            entity.Property(e => e.ProductStatus)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Product_Status");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Inventory__Produ__5441852A");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Members__42A68F2704A2A553");

            entity.Property(e => e.MemberId).HasColumnName("Member_ID");
            entity.Property(e => e.AccessType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Access_Type");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Bmi)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("BMI");
            entity.Property(e => e.CancellationDate).HasColumnName("Cancellation_Date");
            entity.Property(e => e.DateJoined).HasColumnName("Date_Joined");
            entity.Property(e => e.DueDate).HasColumnName("Due_Date");
            entity.Property(e => e.FitnessGoal)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Fitness_Goal");
            entity.Property(e => e.Fname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("FName");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Height).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Lname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("LName");
            entity.Property(e => e.MembershipPeriod)
                .HasMaxLength(255)
                .HasColumnName("Membership_Period");
            entity.Property(e => e.MembershipType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Membership_Type");
            entity.Property(e => e.PaidStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Paid_Status");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(100)
                .HasColumnName("Phone_Number");
            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.Weight).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Staff).WithMany(p => p.Members)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Members__Staff_I__403A8C7D");

            entity.HasOne(d => d.User).WithMany(p => p.Members)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Members__User_ID__3F466844");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Payments__38B787CCCBC02627");

            entity.Property(e => e.ReceiptId).HasColumnName("Receipt_ID");
            entity.Property(e => e.AccessType)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Change).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.InventoryId).HasColumnName("Inventory_ID");
            entity.Property(e => e.Items).HasMaxLength(255);
            entity.Property(e => e.MemberId).HasColumnName("Member_ID");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Payment_Type");
            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");
            entity.Property(e => e.TotalCost)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Total_Cost");
            entity.Property(e => e.TransactionDate).HasColumnName("Transaction_Date");
            entity.Property(e => e.WalkInName).HasMaxLength(255);

            entity.HasOne(d => d.Inventory).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InventoryId)
                .HasConstraintName("FK__Payments__Invent__59063A47");

            entity.HasOne(d => d.Member).WithMany(p => p.Payments)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__Payments__Member__5812160E");

            entity.HasOne(d => d.Staff).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Payments__Staff___571DF1D5");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__9834FBBACB251D5E");

            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .HasColumnName("Product_Name");
            entity.Property(e => e.ProductPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Product_Price");
            entity.Property(e => e.SupplierId).HasColumnName("Supplier_Id");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Products__Suppli__5165187F");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__D80AB49B36427027");

            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Role_name");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staffs__32D1F3C35688645F");

            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");
            entity.Property(e => e.AssignedTask)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(100)
                .HasColumnName("Phone_Number");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Staff)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Staffs__User_ID__3C69FB99");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__83918DB87377D50D");

            entity.Property(e => e.SupplierId).HasColumnName("Supplier_Id");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__206D9190BAC38406");

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.Username).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__Role_ID__398D8EEE");
        });

        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            entity.HasKey(e => e.WorkoutPlanId).HasName("PK__WorkoutP__D9FFB67298176932");

            entity.ToTable("WorkoutPlan");

            entity.Property(e => e.WorkoutPlanId).HasColumnName("WorkoutPlan_ID");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasColumnName("Member_ID");
            entity.Property(e => e.StaffId).HasColumnName("Staff_ID");

            entity.HasOne(d => d.Member).WithMany(p => p.WorkoutPlans)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__WorkoutPl__Membe__5FB337D6");

            entity.HasOne(d => d.Staff).WithMany(p => p.WorkoutPlans)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__WorkoutPl__Staff__60A75C0F");
        });

        modelBuilder.Entity<WorkoutPlanItem>(entity =>
        {
            entity.HasKey(e => e.WorkoutPlanItemId).HasName("PK__WorkoutP__1B6AA810E2480470");

            entity.ToTable("WorkoutPlanItem");

            entity.Property(e => e.WorkoutPlanItemId).HasColumnName("Workout_Plan_ItemID");
            entity.Property(e => e.EquipmentId).HasColumnName("Equipment_ID");
            entity.Property(e => e.ExerciseName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Exercise_Name");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.WorkoutPlanId).HasColumnName("WorkoutPlan_ID");

            entity.HasOne(d => d.Equipment).WithMany(p => p.WorkoutPlanItems)
                .HasForeignKey(d => d.EquipmentId)
                .HasConstraintName("FK__WorkoutPl__Equip__656C112C");

            entity.HasOne(d => d.WorkoutPlan).WithMany(p => p.WorkoutPlanItems)
                .HasForeignKey(d => d.WorkoutPlanId)
                .HasConstraintName("FK__WorkoutPl__Worko__6477ECF3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
