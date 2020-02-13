using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework_Many_to_Many
{

    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }

        public Course()
        {
            StudentCourses = new List<StudentCourse>();
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }

        public Student()
        {
            StudentCourses = new List<StudentCourse>();
        }
    }

    public class StudentCourse
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public ApplicationContext()

        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentCourse>()
                .HasKey(t => new { t.StudentId, t.CourseId });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=relationsdb;Trusted_Connection=True;");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Student s1 = new Student { Name = "Tom" };
                Student s2 = new Student { Name = "Alice" };
                Student s3 = new Student { Name = "Bob" };
                db.Students.AddRange(new List<Student> { s1, s2, s3 });

                Course c1 = new Course { Name = "Алгоритмы" };
                Course c2 = new Course { Name = "Основы программирования" };
                db.Courses.AddRange(new List<Course> { c1, c2 });

                db.SaveChanges();

                // добавляем к студентам курсы
                s1.StudentCourses.Add(new StudentCourse { CourseId = c1.Id, StudentId = s1.Id });
                s2.StudentCourses.Add(new StudentCourse { CourseId = c1.Id, StudentId = s2.Id });
                s2.StudentCourses.Add(new StudentCourse { CourseId = c2.Id, StudentId = s2.Id });
                db.SaveChanges();

                var courses = db.Courses.Include(c => c.StudentCourses).ThenInclude(sc => sc.Student).ToList();
                // выводим все курсы
                foreach (var c in courses)
                {
                    Console.WriteLine($"\n Course: {c.Name}");
                    // выводим всех студентов для данного кура
                    var students = c.StudentCourses.Select(sc => sc.Student).ToList();
                    foreach (Student s in students)
                        Console.WriteLine($"{s.Name}");
                }

                Console.WriteLine();

                var studentsNew = db.Students.Include(c => c.StudentCourses).ThenInclude(sc => sc.Course).ToList();
                // выводим все курсы
                foreach (var News in studentsNew)
                {
                    Console.WriteLine($"\n Student: {News.Name}");
                    // выводим всех студентов для данного кура
                    var newCourses = News.StudentCourses.Select(sc => sc.Course).ToList();
                    foreach (Course c in newCourses)
                        Console.WriteLine($"{c.Name}");
                }

                ////Редактирование
                //Student student = db.Students.Include(s => s.StudentCourses).FirstOrDefault(s => s.Name == "Alice");
                //Course course = db.Courses.FirstOrDefault(c => c.Name == "Алгоритмы");
                //if (student != null && course != null)
                //{
                //    var studentCourse = student.StudentCourses.FirstOrDefault(sc => sc.CourseId == course.Id);
                //    student.StudentCourses.Remove(studentCourse);
                //    db.SaveChanges();
                //}

                ////Удаление
                //Student student = db.Students.FirstOrDefault();
                //db.Students.Remove(student);
                //db.SaveChanges();
            }
        }
    }
}
