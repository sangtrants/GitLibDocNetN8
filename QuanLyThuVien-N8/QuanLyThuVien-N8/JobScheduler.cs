using QuanLyThuVien_N8.Models;
using Quartz;
using Quartz.Impl;
using System;

namespace QuanLyThuVien_N8
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<EmailJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 33))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}