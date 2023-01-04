using System;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using MongoDB.Bson.Serialization.Attributes;
namespace MongoAccess.Model
{
    public class AddSensorUI
    {
        public List<AddCampus> campusList { get; set; }
        public AddSensorUI()
        {
            campusList = new List<AddCampus>();
        }
        public void AddNewCampus(string campus)
        {
            bool found = false;
            foreach (AddCampus ele in campusList)
            {
                if (ele.name == campus)
                    found = true;
            }
            if (!found)
            {
                AddCampus newcampus = new AddCampus();
                newcampus.name = campus;
                campusList.Add(newcampus);
            }
        }
        public void CheckAddBuildings(string campus, string building)
        {
            foreach (AddCampus ele in campusList)
            {
                ele.AddNewBuilding(campus, building);
            }
        }
        public void CheckAddRooms(string campus, string building, string room)
        {
            foreach (AddCampus ele in campusList)
            {
                if (ele.name == campus)
                {
                    foreach (AddBuilding buildingele in ele.buildinglist)
                    {
                        buildingele.AddNewRoom(building, room);
                    }
                }
            }
        }
        public void CheckAddSensors(string campus, string building, string room, string id, string sensorid, string waketimer, string vendor)
        {
            foreach (AddCampus ele in campusList)
            {
                if (ele.name == campus)
                {
                    foreach (AddBuilding buildingele in ele.buildinglist)
                    {
                        if (buildingele.name == building)
                        {
                            foreach (AddRoom roomele in buildingele.roomlist)
                            {
                                roomele.AddNewSensor(campus, building, room, id, sensorid, waketimer, vendor);
                            }
                        }
                    }
                }
            }
        }
    }
    public class AddCampus
    {
        public string name { get; set; }
        public List<AddBuilding> buildinglist { get; set; }
        public AddCampus()
        {
            buildinglist = new List<AddBuilding>();
        }
        public void AddNewBuilding(string campus, string building)
        {
            bool found = false;
            if (name == campus)
            {
                foreach (AddBuilding ele in buildinglist)
                {
                    if (ele.name == building)
                        found = true;
                }
                if (!found)
                {
                    AddBuilding newbuilding = new AddBuilding();
                    newbuilding.name = building;
                    buildinglist.Add(newbuilding);
                }
            }
        }
    }
    public class AddBuilding
    {
        public string name { get; set; }
        public List<AddRoom> roomlist { get; set; }
        public AddBuilding()
        {
            roomlist = new List<AddRoom>();
        }

        public void AddNewRoom(string building, string room)
        {
            bool found = false;
            if (name == building)
            {
                foreach (AddRoom ele in roomlist)
                {
                    if (ele.name == room)
                        found = true;
                }
                if (!found)
                {
                    AddRoom newroom = new AddRoom();
                    newroom.name = room;
                    roomlist.Add(newroom);
                }
            }
        }
    }

    public class AddRoom
    {
        public string name { get; set; }
        public List<SingleSensors> sensorlist { get; set; }
        public AddRoom()
        {
            name = "UNASSIGNED";
            sensorlist = new List<SingleSensors>();
        }

        public void AddNewSensor(string campus, string building, string room, string id, string sensorid, string waketimer, string vendor)
        {
            if (name == room)
            {
                SingleSensors newsensor = new SingleSensors();
                newsensor.id = id.ToUpper();
                newsensor.sensorid = sensorid.ToUpper();
                newsensor.wakeuptimer = waketimer.ToUpper();
                newsensor.vendor = vendor.ToUpper();
                newsensor.campus = campus.ToUpper();
                newsensor.building = building.ToUpper();
                newsensor.room = room.ToUpper();
                newsensor.number = (sensorlist.Count + 1).ToString();
                sensorlist.Add(newsensor);
            }
        }
    }

    public class SingleSensors
    {
        public string id { get; set; }
        public string sensorid { get; set; }
        public string campus { get; set; }
        public string building { get; set; }
        public string room { get; set; }
        public string vendor { get; set; }
        public string wakeuptimer { get; set; }
        public string number { get; set; }
        public SingleSensors()
        {
            id = "UNASSIGNED";
            sensorid = "UNASSIGNED";
            campus = "UNASSIGNED";
            building = "UNASSIGNED";
            room = "UNASSIGNED";
            wakeuptimer = "UNASSIGNED";
            vendor = "UNASSIGNED";
        }
    }

    public class AirConInfo
    {
        public int ID { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Level { get; set; }
        public string Room { get; set; }
        public string AssetName { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNo { get; set; }
        public string SerialNo { get; set; }
    }
    public class SelectedItem
    {
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public List<WarningMessage> messageList { get; set; }
        public void Reset()
        {
            Campus = "Default";
            Building = "Default";
            Room = "Default";
            messageList.Clear();
        }
    }
    public class ScheduleJSONMsg
    {
        public string TStamp { get; set; }
        public int NUM { get; set; }
        public List<string> SCH { get; set; }
        public ScheduleJSONMsg()
        {
            SCH = new List<string>();
            GetUTC1900();
            NUM = 17;
        }
        public void GetUTC1900()
        {
            DateTime t1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diffResult = DateTime.UtcNow - t1900;
            long diff = (long)diffResult.TotalSeconds;
            TStamp = diff.ToString();
        }
    }

    public class ScheduleFirmwareMsg
    {
        /*[0 - 21] [1] for sensor default (priority 0), [2-11] for normal schedules (priority 1), [12-21] for public holiday schedules(priority 2), [22-25] for temporary 1 day(priority 3)*/
        public int scheduleIdx { get; set; }
        public int valid { get; set; }
        public int priority { get; set; } /* [0-3]  0-default, 1- normal, 2- holiday 3- temporary */
        public int startYear { get; set; }   /* (value = start year – 2020 offset), e.g. 2021 will be 1 */
        public int endYear { get; set; }    /* (value = end year – 2020 offset), e.g. 2025 will be 5 */
        public int startMonth { get; set; }  /* from 1Jan) to 12Dec) */
        public int endMonth { get; set; }   /* from 1(Jan) to 12(Dec) */
        public int startDayofMonth { get; set; }   /* 0x00 to 0x1F – 2 ascii bytes */
        public int endDayofMonth { get; set; }     /* send in hex – 2 ascii string bytes – will take 2 ascii */
        public int dayofWeek { get; set; }  /* 1-7 (Mon to Sun)  bit 0 is Monday */
        public int startTime { get; set; } /* schedule start time of the day in minutes from 00:00– e.g 2:30am will be 150 */
        public int endTime { get; set; }   /* schedule end time in min – send in hex – 0x00 to 0x59F */
        public int temperature { get; set; } /* ([16-30] – 15) in hex as ascii	- 16 is 0x0,  30 is 0xE */
        public int fan { get; set; } /* 	[0-5] in ascii	0 for lowest speed and 4 for highset speed 5 auto*/
        public int mode { get; set; } /*	[0-4]  in ascii    0: hot, 1: Cold, 2:dry , 3:fan  4: auto*/
        public int onOff { get; set; }     /* [0-1]	0 for AC off, 1 for AC on */
        public ScheduleFirmwareMsg()
        {
            valid = 1;
            scheduleIdx = 0;
            priority = 1;
            startYear = 2021;
            endYear = 2035;
            startMonth = 1;
            endMonth = 1;
            startDayofMonth = 1;
            endDayofMonth = 1;
            dayofWeek = 0;
            startTime = 0;
            endTime = 0;
            temperature = 22;
            fan = 5;
            mode = 4;
            onOff = 0;
        }
        public string GetScheduleJSONString()
        {
            string oneschedule =
            scheduleIdx.ToString() + "," +
            valid.ToString() + "," +
            priority.ToString() + "," +
            startYear.ToString() + "," +
            endYear.ToString() + "," +
            startMonth.ToString() + "," +
            endMonth.ToString() + "," +
            startDayofMonth.ToString() + "," +
            endDayofMonth.ToString() + "," +
            dayofWeek.ToString() + "," +
            startTime.ToString() + "," +
            endTime.ToString() + "," +
            temperature.ToString() + "," +
            fan.ToString() + "," +
            mode.ToString() + "," +
            onOff.ToString();
            return oneschedule;
        }

        public string GetEmptyScheduleJSONString(int indexofID)
        {
            ScheduleFirmwareMsg res = new ScheduleFirmwareMsg();
            res.valid = 0;
            res.scheduleIdx = indexofID;
            res.priority = 0;
            res.startYear = 0;
            res.endYear = 0;
            res.startMonth = 0;
            res.endMonth = 0;
            res.startDayofMonth = 0;
            res.endDayofMonth = 0;
            res.dayofWeek = 0;
            res.startTime = 0;
            res.endTime = 0;
            res.temperature = 0;
            res.fan = 0;
            res.mode = 0;
            res.onOff = 0;
            return res.GetScheduleJSONString();
        }
    }
    public class PublicHolidayToUI
    {
        public string Date { get; set; }
        public string HolidayName { get; set; }
    }
    public class PublicHolidayRecord
    {
        public DateTime Date { get; set; }
        public Boolean Expired { get; set; }
        public string HolidayName { get; set; }
        public PublicHolidayToUI GetStringToUI()
        {
            PublicHolidayToUI res = new PublicHolidayToUI();
            res.Date = Date.ToString("dd/MM/yyyy");
            res.HolidayName = HolidayName;
            return res;
        }
    }
    public class PublicHoliday
    {
        /*"_id":97,
      "Date":"20211225",
      "Holiday Name":"Christmas Day",
      "Information":"Christmas Day is an annual holiday which celebrates the birth of Jesus Christ over 2000 years ago.",
      "More Information":"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021",
      "Jurisdiction":"vic",
      "rank":0.0573088*/
        public int _id { get; set; }
        public string Date { get; set; }
        public string HolidayName { get; set; }
        public string Information { get; set; }
        public string MoreInformation { get; set; }
        public string Jurisdiction { get; set; }
        public float rank { get; set; }
        public PublicHolidayRecord GetShortRecord()
        {
            PublicHolidayRecord res = new PublicHolidayRecord();
            res.Date = DateTime.ParseExact(Date, "yyyyMMdd",
                                  CultureInfo.InvariantCulture);
            res.HolidayName = HolidayName;
            if (DateTime.Compare(res.Date, DateTime.Now) < 0)
                res.Expired = true;
            else
                res.Expired = false;
            return res;
        }
    }
    public class HolidayJson
    {
        public List<PublicHoliday> holidaylist { get; set; }
        public HolidayJson()
        {
            holidaylist = new List<PublicHoliday>();
        }
    }
    public class ScheduleResult
    {
        public ObjectId _id { get; set; }
        public DateTime Timestamp { get; set; }
        public ScheduleManager ScheduleBlob { get; set; }
        public SelectionModel Selecter { get; set; }
    }

    public class ScheduleUIJson
    {
        public string Id { get; set; }
        public string Topic { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string Timestamp { get; set; }
        public string Priority { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string WeekDays { get; set; }
        public string Temperature { get; set; }
        public string FanSpeed { get; set; }
        public string Mode { get; set; }
        public string ResetTimer { get; set; }
        public string PowerStatus { get; set; }
    }

    public class Schedule
    {
        public DateTime TimeStamp { get; set; }
        public int ScheduleIdx { get; set; }
        public int Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int WeekDays { get; set; }
        public int Temperature { get; set; }
        public int FanSpeed { get; set; }
        public string Mode { get; set; }
        public int ResetTimer { get; set; }
        public Boolean PowerStatus { get; set; }
        public int Compare(DateTime d1, DateTime d2)
        {
            return DateTime.Compare(d1, d2);

            // < 0 − If date1 is earlier than date2
            // 0 − If date1 is the same as date2
            // > 0 − If date1 is later than date2
        }

        public bool isSame(Schedule another)
        {
            bool res = false;
            if ((another.Priority == Priority)
                && (another.StartDate == StartDate) &&
                (another.EndDate == EndDate) &&
                (another.StartTime.Hour == StartTime.Hour) &&
                 (another.StartTime.Minute == StartTime.Minute) &&
                   (another.EndTime.Hour == EndTime.Hour) &&
                 (another.EndTime.Minute == EndTime.Minute) &&
                (another.Temperature == Temperature) &&
                (another.FanSpeed == FanSpeed) &&
                (another.ResetTimer == ResetTimer) &&
                (another.PowerStatus == PowerStatus))
                res = true;
            return res;
        }
        public void CheckSchedulePriority()
        {
            if (Priority == -123)
            {
                return;  //deleted entry
            }
            if (Compare(EndDate.AddHours(24), DateTime.Now) < 0)
            {
                Priority = -1;  //expired                 
            }
            else if (Compare(StartDate, EndDate) < 0)
            {
                if (Priority != 2)
                {
                    //not holiday by default it is 1
                    Priority = 1;
                    TimeSpan difference = EndDate - StartDate;
                    int differentdays = difference.Days;
                    //check very short of setting
                    if (differentdays <= 2)
                    {
                        Priority = 3; //temp setting 
                    }
                    TimeSpan timedif = EndTime - StartTime;
                    if (timedif.Hours <= 4)
                    {
                        Priority = 3; //long term but still < 4h, so put as temp priority 
                    }
                }
            }
            else if (Compare(StartDate, EndDate) == 0)
            {
                if (Priority != 2)
                    Priority = 3; //temporary, same day , but not holiday, so it is common day, so set to be high priority 
            }
            else if (Compare(StartDate, EndDate) > 0)
            {
                //wrong setting, end day earlier then start day, will not implement
                Priority = -2;
            }
            //check time
            if (Compare(StartTime, EndTime) > 0)
            {
                //wrong setting start time later than end time, will not implement 
                Priority = -3;
            }
            else if (Compare(StartTime, EndTime) == 0)
            {
                //wrong setting start time same as end time, will not implement 
                if (Priority != 2)
                    Priority = -4;
            }
        }
        public Schedule()
        {
            ScheduleIdx = 0;
            Priority = 1;
            Temperature = 22;
            FanSpeed = 5;
            Mode = "AUTO";
            ResetTimer = 3;
            PowerStatus = false;
            TimeStamp = DateTime.Now;
            WeekDays = 0;
        }
        public Schedule GetCopy()
        {
            Schedule newcopy = new Schedule();
            newcopy.TimeStamp = TimeStamp;
            newcopy.ScheduleIdx = ScheduleIdx;
            newcopy.Priority = Priority;
            newcopy.StartDate = StartDate;
            newcopy.EndDate = EndDate;
            newcopy.StartTime = StartTime;
            newcopy.EndTime = EndTime;
            newcopy.WeekDays = WeekDays;
            newcopy.Temperature = Temperature;
            newcopy.FanSpeed = FanSpeed;
            newcopy.Mode = Mode;
            newcopy.ResetTimer = ResetTimer;
            newcopy.PowerStatus = PowerStatus;
            return newcopy;
        }
        public ScheduleFirmwareMsg GetScheduleFirmwareMsg()
        {
            ScheduleFirmwareMsg msg = new ScheduleFirmwareMsg();
            msg.valid = 0;
            if (Priority >= 0)
                msg.valid = 1;
            msg.priority = Priority;
            msg.startYear = StartDate.Year;
            msg.endYear = EndDate.Year;
            msg.startMonth = StartDate.Month;
            msg.endMonth = EndDate.Month;
            msg.startDayofMonth = StartDate.Day;
            msg.endDayofMonth = EndDate.Day;
            msg.dayofWeek = WeekDays;
            msg.startTime = StartTime.Hour * 60 + StartTime.Minute;
            msg.endTime = EndTime.Hour * 60 + EndTime.Minute;
            msg.temperature = Temperature;
            msg.fan = FanSpeed;
            if (Mode == "AUTO")
                msg.mode = 4;
            if (Mode == "FAN")
                msg.mode = 3;
            if (Mode == "DRY")
                msg.mode = 2;
            if (Mode == "COOL")
                msg.mode = 1;
            if (Mode == "HEAT")
                msg.mode = 0;
            if (PowerStatus)
                msg.onOff = 1;
            else
                msg.onOff = 0;
            return msg;
        }
        public String GetUIMsg()
        {
            string power = "ON";
            if (!PowerStatus)
                power = "OFF";

            string res = " Id: " + ScheduleIdx + ", " + TimeStamp.ToShortDateString() +
              " Priority: " + Priority + ", From " + StartDate.ToShortDateString() + " To " + EndDate.ToShortDateString() + " Between " + StartTime.ToShortTimeString() + " And " + EndTime.ToShortTimeString() + ", Set Temp: " + Temperature + ", Fan Speed: " + FanSpeed + ", Mode: " + Mode + ", ResetTimer: " + ResetTimer + ", PowerStatus: " + power;
            return res;
        }
        public string convertToBinary(int b)
        {
            Boolean[] binary = new Boolean[8];
            Boolean bin;
            for (int i = 0; i <= 6; i++)
            {
                if (b % 2 == 1) bin = true;
                else bin = false;
                binary[i] = bin;
                b /= 2;
            }
            string res = "";
            for (int i = 0; i < 8; i++)
            {
                if (i == 0)
                {
                    if (binary[i] == true)
                        res += "Su,";
                }
                if (i == 1)
                {
                    if (binary[i] == true)
                        res += "M,";
                }
                if (i == 2)
                {
                    if (binary[i] == true)
                        res += "Tu,";
                }
                if (i == 3)
                {
                    if (binary[i] == true)
                        res += "We,";
                }
                if (i == 4)
                {
                    if (binary[i] == true)
                        res += "Th,";
                }
                if (i == 5)
                {
                    if (binary[i] == true)
                        res += "F,";
                }
                if (i == 6)
                {
                    if (binary[i] == true)
                        res += "Sa, ";
                }
            }
            return res;
        }
        public ScheduleUIJson GetScheduleUIJson()
        {
            ScheduleUIJson newjson = new ScheduleUIJson();
            newjson.Timestamp = TimeStamp.ToLocalTime().ToString("dd/MMM/yyyy") + "  " + TimeStamp.ToLocalTime().ToShortTimeString();
            newjson.Priority = "Deleted";
            if (Priority == 0)
                newjson.Priority = "Default";
            if (Priority == 1)
                newjson.Priority = "Daily";
            if (Priority == 2)
                newjson.Priority = "Holiday";
            if (Priority == 3)
                newjson.Priority = "Temporary";
            newjson.StartDate = StartDate.ToLocalTime().ToString("dd/MM/yyyy");
            newjson.EndDate = EndDate.ToLocalTime().ToString("dd/MM/yyyy");
            newjson.StartTime = StartTime.ToLocalTime().ToShortTimeString();
            newjson.EndTime = EndTime.ToLocalTime().ToShortTimeString();
            newjson.WeekDays = WeekDays.ToString();
            newjson.Temperature = Temperature.ToString();

            //  0": "Speed 1", "1": "Speed 2", "2": "Speed 3", "3": "Speed 4", "4": "Speed 5", "5": "Auto", "6": "Silent"

            if (FanSpeed == 0)
                newjson.FanSpeed = "SPEED_1";
            if (FanSpeed == 1)
                newjson.FanSpeed = "SPEED_2";
            if (FanSpeed == 2)
                newjson.FanSpeed = "SPEED_3";
            if (FanSpeed == 3)
                newjson.FanSpeed = "SPEED_4";
            if (FanSpeed == 4)
                newjson.FanSpeed = "SPEED_5";
            if (FanSpeed == 5)
                newjson.FanSpeed = "AUTO";
            if (FanSpeed == 6)
                newjson.FanSpeed = "SILENT"; 
            newjson.Mode = Mode.ToString();
            newjson.ResetTimer = ResetTimer.ToString();
            newjson.PowerStatus = PowerStatus ? "ON" : "OFF";
            newjson.WeekDays = convertToBinary(WeekDays);
            return newjson;
        }
    }
    public class TopicSchedule
    {
        public string topic { get; set; }
        public List<Schedule> ScheduleList { get; set; }
        public List<ScheduleFirmwareMsg> ScheduleFirmwareMsgList;
        public List<string> ScheduleJSONListString;
        public TopicSchedule()
        {
            topic = "";
            ScheduleList = new List<Schedule>();
            ScheduleFirmwareMsgList = new List<ScheduleFirmwareMsg>();
            ScheduleJSONListString = new List<string>();
        }

        public TopicSchedule(string mytopic, List<Schedule> MyScheduleList)
        {
            topic = "";
            if (!String.IsNullOrEmpty(mytopic))
            {
                topic = mytopic;
            }
            ScheduleList = new List<Schedule>();
            if (MyScheduleList.Count > 0)
            {
                ScheduleList = MyScheduleList;
            }
            ScheduleFirmwareMsgList = new List<ScheduleFirmwareMsg>();
            ScheduleJSONListString = new List<string>();
        }


        public void CheckExpire()
        {
            foreach (Schedule task in ScheduleList)
            {
                task.CheckSchedulePriority();
            }
        }
        public ScheduleJSONMsg GenerateFirmwareMsg(int group1indexinput, int group2indexinput, int group3indexinput, int lastindex)
        {
            ScheduleJSONMsg payload = new ScheduleJSONMsg();
            ScheduleFirmwareMsgList.Clear();
            ScheduleJSONListString.Clear();
            CheckExpire();
            if (ScheduleList.Count > 0)
            {
                foreach (Schedule sch in ScheduleList)
                {
                    ScheduleFirmwareMsg msg = sch.GetScheduleFirmwareMsg();
                    ScheduleFirmwareMsgList.Add(msg);
                }
                int group1index = group1indexinput;
                int group2index = group2indexinput;
                int group3index = group3indexinput;
                ScheduleFirmwareMsg zero = new ScheduleFirmwareMsg();
                string defaultschedule = zero.GetEmptyScheduleJSONString(0);
                ScheduleJSONListString.Add(defaultschedule);
                for (int i = 1; i <= lastindex; i++)
                {
                    string empty = zero.GetEmptyScheduleJSONString(i);
                    ScheduleJSONListString.Add(empty);
                }
                foreach (ScheduleFirmwareMsg ele in ScheduleFirmwareMsgList)
                {
                    if (ele.priority == 1)
                    {
                        if (group1index < group2index)
                        {
                            ele.scheduleIdx = group1index;
                            group1index++;
                        }
                        else
                        {
                            ele.scheduleIdx = -1;
                        }
                    }
                    if (ele.priority == 2)
                    {
                        if (group2index < group3index)
                        {
                            ele.scheduleIdx = group2index;
                            group2index++;
                        }
                        else
                        {
                            ele.scheduleIdx = -2;
                        }
                    }
                    if (ele.priority == 3)
                    {
                        if (group3index < lastindex + 1)
                        {
                            ele.scheduleIdx = group3index;
                            group3index++;
                        }
                        else
                        {
                            ele.scheduleIdx = -3;
                        }
                    }
                }
                foreach (ScheduleFirmwareMsg ele in ScheduleFirmwareMsgList)
                {
                    if (ele.scheduleIdx >= 1)
                    {
                        string schedulestring = ele.GetScheduleJSONString();
                        ScheduleJSONListString[ele.scheduleIdx] = schedulestring;
                    }
                }
            }
            payload.SCH = ScheduleJSONListString;
            payload.NUM = payload.SCH.Count;
            return payload;
        }
    }
    public class ScheduleManager
    {
        public List<Schedule> ScheduleList;
        public List<TopicSchedule> ListOfTopicAllSchedule { get; set; }

        public List<string> ScheduleListUIString;
        public string UIstringsingle;
        public void CheckExpire()
        {
            foreach (Schedule task in ScheduleList)
            {
                task.CheckSchedulePriority();
            }
        }
        public void GenerateScheduleUIMsg()
        {
            ScheduleListUIString.Clear();
            CheckExpire();
            UIstringsingle = "";
            foreach (Schedule ele in ScheduleList)
            {
                string schedulemsg = ele.GetUIMsg();
                ScheduleListUIString.Add(schedulemsg);
                UIstringsingle += schedulemsg + "|";
            }
        }
        public ScheduleManager()
        {
            ScheduleList = new List<Schedule>();
            ListOfTopicAllSchedule = new List<TopicSchedule>();
            ScheduleListUIString = new List<string>();
        }
        public void AddSchedule(int priority, string startdate, string enddate, string starttime, string endtime, int weekday, int temp, string mode, int fan, int timer, bool power)
        {
            CultureInfo enAU = new CultureInfo("en-AU");
            DateTime sdate = DateTime.Parse(startdate, enAU);
            DateTime edate = DateTime.Parse(enddate, enAU);
            DateTime stime;
            DateTime etime;

            if (starttime != "now" && endtime != "now")
            {
                stime = DateTime.Parse(starttime);
                etime = DateTime.Parse(endtime);
            }
            else
            {
                stime = new DateTime(sdate.Year, sdate.Month, sdate.Day, 0, 5, 0);
                etime = new DateTime(sdate.Year, sdate.Month, sdate.Day, 23, 59, 0);
            }
            Schedule scheduleEntry = new Schedule();
            scheduleEntry.ScheduleIdx = ScheduleList.Count + 1;
            scheduleEntry.Priority = priority;
            scheduleEntry.StartDate = sdate;
            scheduleEntry.EndDate = edate;
            scheduleEntry.StartTime = stime;
            scheduleEntry.EndTime = etime;
            scheduleEntry.WeekDays = weekday;
            scheduleEntry.Temperature = temp;
            scheduleEntry.Mode = mode;
            scheduleEntry.FanSpeed = fan;
            scheduleEntry.ResetTimer = timer;
            scheduleEntry.PowerStatus = power;
            ScheduleList.Add(scheduleEntry);
        }

        public void AppendScheduleList(List<Schedule> NewScheduleList)
        {
            for (int i = 0; i < NewScheduleList.Count; i++)
            {
                ScheduleList.Add(ScheduleList[i]);
            }
        }
    }
    public class NetZero
    {
        public List<Campus> Campuses { get; set; }
        public NetZero()
        {
            Campuses = new List<Campus>();
        }

        public void ClearAllBuildings()
        {
            foreach (Campus ele in Campuses)
            {
                ele.Buildings.Clear();
            }
        }
        public void Clear()
        {
            Campuses.Clear();
        }
    }
    public class Campus
    {
        public string Name;
        public List<Buildings> Buildings { get; set; }
        public Campus()
        {
            Buildings = new List<Buildings>();
        }

        public void ClearAllRooms()
        {
            foreach (Buildings ele in Buildings)
            {
                ele.ClearRoom();
            }
        }
    }
    public class Buildings
    {
        public string Name;
        public List<string> Rooms { get; set; }
        public List<WarningMessage> RoomDataList { get; set; }
        public Buildings()
        {
            Rooms = new List<string>();
            RoomDataList = new List<WarningMessage>();
        }

        public void AddRoom(WarningMessage roomdata)
        {
            RoomDataList.Add(roomdata);
            Rooms.Add(roomdata.Room);
        }

        public void ClearRoom()
        {
            RoomDataList.Clear();
            Rooms.Clear();
        }
    }
    public class TopicStringArray
    {
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string SensorId { get; set; }
        public int Pos { get; set; }
        public TopicStringArray(string campus, string building, string room, string sensorId, int pos)
        {
            Campus = campus;
            Building = building;
            Room = room;
            SensorId = sensorId;
            Pos = pos;
        }
        public string GetTopicString()
        {
            return "AU/" + Campus.ToUpper() + "/" + Building.ToUpper() + "/" + Room.ToUpper() + "/" + SensorId.ToUpper() + "/" + Pos;
        }
    }

    public class SelectionModel
    {
        public List<string> TopicList { get; set; }
        public NetZero NetZeroNetwork { get; set; }
        public string topicstringSingle { get; set; }
        public SelectionModel()
        {
            NetZeroNetwork = new NetZero();
            TopicList = new List<string>();
        }
        public void GenerateTopicList()
        {
            TopicList.Clear();
            topicstringSingle = "";

            if (NetZeroNetwork.Campuses.Count == 0)
            {
                //no any campus selected, just return
                return;
            }
            foreach (Campus ele in NetZeroNetwork.Campuses)
            {
                if (ele.Buildings.Count == 0)
                {
                    //no building selected, all campus, prepare a new topic for all campus
                    TopicStringArray newTopic = new TopicStringArray(ele.Name, "ALL", "ALL", "ALL", 1);
                    //for all campus 
                    TopicList.Add(newTopic.GetTopicString());
                }
                else
                {
                    //has building selected
                    foreach (Buildings building in ele.Buildings)
                    {
                        if (building.Rooms.Count == 0)
                        {
                            //no rooms in this building, so all buildings, create for all buildings
                            TopicStringArray newTopic = new TopicStringArray(ele.Name, building.Name, "ALL", "ALL", 2);
                            TopicList.Add(newTopic.GetTopicString());
                        }
                        else
                        {
                            //this building has room selected
                            foreach (string rooms in building.Rooms)
                            {
                                TopicStringArray newTopic = new TopicStringArray(ele.Name, building.Name, rooms, "ALL", 3);
                                TopicList.Add(newTopic.GetTopicString());
                            }
                        }
                    }
                }
            }
            foreach (string ele in TopicList)
            {
                topicstringSingle += ele + " | ";
            }
        }
        public void Clear()
        {
            TopicList.Clear();
            NetZeroNetwork.Clear();
        }
    }
    public class WarningMessage
    {
        public string SensorId { get; set; }
        public string NickName { get; set; }
        public string Timestamp { get; set; }
        public string Vendor { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string AirConNo { get; set; }
        public string Note { get; set; }
        public string SetTemperature { get; set; }
        public string RoomTemperature { get; set; }
        public string Humidity { get; set; }
        public string FanSpeed { get; set; }
        public string Mode { get; set; }
        public string PowerOnOff { get; set; }
        public string BatteryPercentage { get; set; }
        public string Reason { get; set; }
        public string ConfigHierarchy { get; set; }
    }
    /*  public class TimeSeriesObjectForJson
      {
          public string SensorId { get; set; }
          public string Timestamp { get; set; }
          public string Temperature { get; set; }
          public string FanSpeed { get; set; }
          public bool PowerOnOff { get; set; }
          public string TopicName { get; set; }
          public string Note { get; set; }
      }*/

    /* public class TimeSeriesObjectForJsonSent
     {
         public string SensorId { get; set; }
         public string TStamp { get; set; }
         public string STemp { get; set; }
         public string Fan { get; set; }
         public string Mode { get; set; }
         public string OnOff { get; set; }
         public string D3 { get; set; }
         public string CMD { get; set; }
     }*/


    public class TimeSeriesObjectForJsonSentSCfg
    {
        public string SensorId { get; set; }
        public string TStamp { get; set; }
        public Int32 WkTime { get; set; }
        public Int32 ConnTime { get; set; }
        public string D3 { get; set; }
        public string CMD { get; set; }
    }

    public class TimeSeriesObject
    {
        public ObjectId _id { get; set; }
        public string SensorId { get; set; }
        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Timestamp { get; set; }
        public UInt32 Utc { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string DayOfWeek { get; set; }
        public int Week { get; set; }
        public int Hour { get; set; }
        public int Min { get; set; }
        public int Sec { get; set; }
        public int SetTemperature { get; set; }
        public int RoomTemperature { get; set; }
        public int FanSpeed { get; set; }
        public string Mode { get; set; }
        public int ResetTimer { get; set; }
        public bool PowerOnOff { get; set; }
        public int BatteryPercentage { get; set; }
        public int Rssi { get; set; }
        public int Humidity { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public ScheduleManager ScheduleBlob { get; set; }
        public SelectionModel Selecter { get; set; }
        public string Command { get; set; }
        DateTime GetLocalTime()
        {
            DateTime thistime = DateTime.Now;
            //  TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            //  DateTime tstTime = TimeZoneInfo.ConvertTime(thistime, tst);
            /* if (tstTime.IsDaylightSavingTime())
             {
                 TimeZoneInfo tstDayLight = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Daylight Time");
                 tstTime = TimeZoneInfo.ConvertTime(thisTime, tstDayLight);
             }*/
            return thistime;
        }
        public TimeSeriesObject()
        {
            _id = ObjectId.GenerateNewId();
            Timestamp = GetLocalTime();
            Utc = (UInt32)_id.Timestamp;
            Year = Timestamp.Year;
            Month = Timestamp.Month;
            Day = Timestamp.Day;
            DayOfWeek = Timestamp.DayOfWeek.ToString();
            CultureInfo myCI = new CultureInfo("en-AU");
            Calendar myCal = myCI.Calendar;
            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            Week = myCal.GetWeekOfYear(Timestamp, myCWR, myFirstDOW);
            Hour = Timestamp.Hour;
            Min = Timestamp.Minute;
            Sec = Timestamp.Second;
            Mode = "AUTO";
            SetTemperature = 22;
            RoomTemperature = -1;
            FanSpeed = 3;
            PowerOnOff = true;
            BatteryPercentage = -1;
            ResetTimer = 3;
            Rssi = 0;
            D1 = "";
            D2 = "";
            D3 = "";
            Command = "";
            ScheduleBlob = new ScheduleManager();
        }
        public void SetValue(int temp, int fan_speed, string mode_default, bool power_onoff, int reset_timer, ScheduleManager schedule_blob, SelectionModel MySelecter, string CMD)
        {
            SetTemperature = temp;
            FanSpeed = fan_speed;
            Mode = mode_default;
            PowerOnOff = power_onoff;
            ResetTimer = reset_timer;
            ScheduleBlob = schedule_blob;
            Selecter = MySelecter;
            Command = CMD;
            if (CMD == "updatesensortopic")
                Command = "SCfg";
            if (CMD == "reboot")
                Command = "Rst";
        }
        public void SetHierarchy(string _campus, string _building, string _room, int level)
        {
            if (level != 1 || level != 2 || level != 3)
                return;
            TopicStringArray newTopic = new TopicStringArray(_campus, _building, _room, "ALL", level);
            string Topic = newTopic.GetTopicString();
            D3 = Topic;
        }

        public List<Schedule> AppendNewScheduleList(TimeSeriesObject newobj)
        {
            List<Schedule> mynewlist = new List<Schedule>();
            foreach (Schedule ele in ScheduleBlob.ScheduleList)
            {
                Schedule elecopy = ele.GetCopy();
                mynewlist.Add(elecopy);
            }

            foreach (Schedule ele in newobj.ScheduleBlob.ScheduleList)
            {
                Schedule elecopy = ele.GetCopy();
                mynewlist.Add(elecopy);
            }
            return mynewlist;
        }
        public string GetUTC1900()
        {
            DateTime t1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diffResult = DateTime.UtcNow - t1900;
            long diff = (long)diffResult.TotalSeconds;
            return diff.ToString();
        }

        public TimeSeriesObjectForJsonSentSCfg ConvertToTimeSeriesObjectForJsonSentSCfg(int wakeuptimer)
        {
            return new TimeSeriesObjectForJsonSentSCfg
            {
                SensorId = SensorId,
                TStamp = GetUTC1900(),
                WkTime = wakeuptimer,
                ConnTime = 60,
                D3 = D3,
                /*STemp = SetTemperature > 18 && SetTemperature < 25 ? SetTemperature.ToString() : null,
                Fan = FanSpeed >= 0 && FanSpeed < 6 ? FanSpeed.ToString() : null,
                Mode = Mode,
                OnOff = PowerOnOff ? "On" : "Off",
                D3 = D3,*/
                CMD = Command
            };
        }

        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToLocalTime().ToShortDateString() + " " + DateTime.Now.ToLocalTime().ToLongTimeString();
        }

        public string GetTimestampDateTime()
        {
            return Timestamp.ToShortDateString() + " " + Timestamp.ToLongTimeString();
        }
        public string GetLatestConfigInfo()
        {
            string datetime = GetTimestampDateTime();
            string power = " ON ";
            if (!PowerOnOff)
                power = " OFF ";
            string res = datetime + "- SetTemp:[" + SetTemperature.ToString() + "] FanSpeed:[" + FanSpeed.ToString() + "] Mode:[" + Mode + "] Power:[" + power + "]";
            return res;
        }

        /* public Dictionary<string, string> GetDbProperties()
         {
             return new Dictionary<string, string>
             {
                 { "TopicName", TopicName },
                 { "Temperature", Temperature.ToString()},
                 { "FanSpeed", FanSpeed.ToString()},
                 { "DateTime", GetCurrentDateTime() },
                 { "PowerOnOff", PowerOnOff.ToString() },
                 { "Note", Note},
             };
         }*/
    }
    public class DeviceObject
    {
        public ObjectId _id { get; set; }
        public string SensorId { get; set; }
        public string NickName { get; set; }
        public string Vendor { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string AirConNo { get; set; }
        public string IRCode { get; set; }
        public string Note { get; set; }
        public string ConfigHierarchy { get; set; } //0 - AU, 1 Campus 2 Building 3 Room 4 Specific Sensor level (means this is set by the higher level) 
        public string ConfigInfo { get; set; }
        //public Dictionary<string, string> CurrentProperties { get; set; }
        //public Dictionary<string, string> RequestProperties { get; set; }
        public TimeSeriesObject CurrentProperties { get; set; }
        public TimeSeriesObject RequestProperties { get; set; }
        public DeviceObject()
        {
            SensorId = "";
            NickName = "";
            Vendor = "";
            Campus = "";
            Building = "";
            Room = "";
            AirConNo = "0";
            IRCode = "";
            Note = "60";
            ConfigHierarchy = "UNASSIGNED";
            ConfigInfo = "UNASSIGNED";
            CurrentProperties = new TimeSeriesObject();
            RequestProperties = new TimeSeriesObject();
        }
        public void SetConfigHierarchy(string configHierarchy)
        {
            string config = RequestProperties.GetLatestConfigInfo();
            if ((configHierarchy != "UNASSIGNED"))
            {
                ConfigHierarchy = configHierarchy;
                string hierarchyName = "";
                if (ConfigHierarchy == "Campus")
                    hierarchyName = Campus;
                if (ConfigHierarchy == "Building")
                    hierarchyName = Building;
                if (ConfigHierarchy == "Room")
                    hierarchyName = Room;
                ConfigInfo = " | " + ConfigHierarchy + " [" + hierarchyName + "] Override Setting " + config;
            }
        }
        public WarningMessage GeWarningMessages()
        {
            WarningMessage res = new WarningMessage();
            res.SensorId = SensorId;
            res.Vendor = Vendor;
            res.Campus = Campus;
            res.Building = Building;
            res.Room = Room;
            res.NickName = NickName;
            res.Note = Note;
            res.AirConNo = AirConNo.ToString();
            res.ConfigHierarchy = ConfigHierarchy.ToString();

            if (!(CurrentProperties is null))
            {
                res.Timestamp = CurrentProperties.Timestamp.ToString("HH:mm dd/MM/yy");
                res.SetTemperature = CurrentProperties.SetTemperature.ToString() + "°C";
                res.RoomTemperature = CurrentProperties.RoomTemperature.ToString()+ "°C";
                res.Humidity = CurrentProperties.Humidity.ToString()+"%"; 
                res.FanSpeed = CurrentProperties.FanSpeed.ToString();
               
                res.PowerOnOff = CurrentProperties.PowerOnOff ? "ON" : "OFF";
                /* double percentage = (CurrentProperties.BatteryPercentage / 60);  
                 int percent = (int) percentage;
                 if (percent > 100)
                     percent = 100; 
                 •	Level 75 % = 5.1V
                 •	Level 50 %   = 4.9V
                 •	Level 20 % = 4.5V
                 •	Level 0 % = 4.0 V*/
                string battery = "GOOD";
                int level = CurrentProperties.BatteryPercentage;
                if (level <= 5100 && level > 4900)
                {
                    battery = "MEDIUM";
                }
                if (level <= 4900 && level > 4500)
                {
                    battery = "LOW";
                }
                if (level <= 4500)
                {
                    battery = "DEAD";
                } 
                res.BatteryPercentage = battery.ToString();
                // "A": "Auto", "D": "Dry", "C": "Cool", "H": "Heat", "F": "Fan Only"
              
                res.Mode = CurrentProperties.Mode.ToUpper();
                //"SPEED_1": "Speed 1", "SPEED_2": "Speed 2", "SPEED_3": "Speed 3", "SPEED_4": "Speed 4", "SPEED_5": "Speed 5", "AUTO": "Auto", "SILENT": "Silent" }
                if (CurrentProperties.FanSpeed == 0)
                    res.FanSpeed = "SPEED_1";
                if (CurrentProperties.FanSpeed == 1)
                    res.FanSpeed = "SPEED_2";
                if (CurrentProperties.FanSpeed == 2)
                    res.FanSpeed = "SPEED_3";
                if (CurrentProperties.FanSpeed == 3)
                    res.FanSpeed = "SPEED_4";
                if (CurrentProperties.FanSpeed == 4)
                    res.FanSpeed = "SPEED_5";
                if (CurrentProperties.FanSpeed == 5)
                    res.FanSpeed = "AUTO";
                if (CurrentProperties.FanSpeed == 6)
                    res.FanSpeed = "SILENT";

                res.Reason = "";
            }
            return res;
        }

        public WarningMessage GeWarningMessages_API()
        {
            WarningMessage res = new WarningMessage();
            res.SensorId = SensorId;
            res.Vendor = Vendor;
            res.Campus = Campus;
            res.Building = Building;
            res.Room = Room;
            res.NickName = NickName;
            res.Note = Note;
            res.AirConNo = AirConNo.ToString();
            res.ConfigHierarchy = ConfigHierarchy.ToString();

            if (!(CurrentProperties is null))
            {
                res.Timestamp = CurrentProperties.Timestamp.ToString("HH:mm dd/MM/yy");
                res.SetTemperature = CurrentProperties.SetTemperature.ToString() ;
                res.RoomTemperature = CurrentProperties.RoomTemperature.ToString();
                res.Humidity = CurrentProperties.Humidity.ToString();
                res.FanSpeed = CurrentProperties.FanSpeed.ToString();
                res.PowerOnOff = CurrentProperties.PowerOnOff ? "ON" : "OFF";
                /* double percentage = (CurrentProperties.BatteryPercentage / 60);  
                 int percent = (int) percentage;
                 if (percent > 100)
                     percent = 100; 
                 •	Level 75 % = 5.1V
                 •	Level 50 %   = 4.9V
                 •	Level 20 % = 4.5V
                 •	Level 0 % = 4.0 V*/
               
                res.BatteryPercentage = CurrentProperties.BatteryPercentage.ToString();
  
                // "A": "Auto", "D": "Dry", "C": "Cool", "H": "Heat", "F": "Fan Only"

                res.Mode = CurrentProperties.Mode.ToUpper();
                //"SPEED_1": "Speed 1", "SPEED_2": "Speed 2", "SPEED_3": "Speed 3", "SPEED_4": "Speed 4", "SPEED_5": "Speed 5", "AUTO": "Auto", "SILENT": "Silent" }
                if (CurrentProperties.FanSpeed == 0)
                    res.FanSpeed = "SPEED_1";
                if (CurrentProperties.FanSpeed == 1)
                    res.FanSpeed = "SPEED_2";
                if (CurrentProperties.FanSpeed == 2)
                    res.FanSpeed = "SPEED_3";
                if (CurrentProperties.FanSpeed == 3)
                    res.FanSpeed = "SPEED_4";
                if (CurrentProperties.FanSpeed == 4)
                    res.FanSpeed = "SPEED_5";
                if (CurrentProperties.FanSpeed == 5)
                    res.FanSpeed = "AUTO";
                if (CurrentProperties.FanSpeed == 6)
                    res.FanSpeed = "SILENT";

                res.Reason = "";
            }
            return res;
        }

    }

    public class TimeSeriesObjectForJsonRecv
    {
        public string SensorId { get; set; }
        public string TStamp { get; set; } //1900 utc time 
        public Int32 RTemp { get; set; }  //room sensor temperature 
        public Int32 Bat { get; set; } //0-6000 mv  lower than 4000mv is dead, 4800mv is warning 
        public Int32 STemp { get; set; } //set tempeature, offsets 5 degree 
        public Int32 Fan { get; set; }
        public Int32 Mode { get; set; }
        public Int32 OnOff { get; set; } // 0 and 1 
        public Int32 Rssi { get; set; } //not use 
        public Int32 RoomRH { get; set; } //0-100 % 
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string CMD { get; set; } //SSts

        /*   public TimeSeriesObjectForJson ConvertToModel()
           {
               return new TimeSeriesObjectForJson
               {
                   SensorId = SensorId,
                   Timestamp = Timestamp,
                   Temperature = Temperature,
                   FanSpeed = FanSpeed,
                   PowerOnOff = Convert.ToBoolean(PowerOnOff),
                   TopicName = TopicName,
                   Note = Note
               };
           }*/

        public TimeSeriesObject ConvertToTimeSeriesObj()
        {
            TimeSeriesObject res = new TimeSeriesObject();
            res.SensorId = SensorId;
            res.Timestamp = DateTime.Now.ToLocalTime();
            res.SetTemperature = STemp;
            res.RoomTemperature = RTemp;
            res.FanSpeed = Fan;
            res.Mode = "";
            if (Mode == 0)
                res.Mode = "HEAT";
            if (Mode == 1)
                res.Mode = "COOL";
            if (Mode == 2)
                res.Mode = "DRY";
            if (Mode == 3)
                res.Mode = "FAN";
            if (Mode == 4)
                res.Mode = "AUTO";
            res.PowerOnOff = Convert.ToBoolean(OnOff);
            res.BatteryPercentage = Bat;
            res.Rssi = Rssi;
            if (RoomRH > 100)
                RoomRH = 100;
            res.Humidity = RoomRH;

            res.D1 = D1;
            res.D2 = D2;
            res.D3 = D3;
            res.Command = CMD;

            if (string.IsNullOrEmpty(res.RoomTemperature.ToString()))
                res.RoomTemperature = -1;
            if (string.IsNullOrEmpty(res.SetTemperature.ToString()))
                res.SetTemperature = -1;
            if (string.IsNullOrEmpty(res.FanSpeed.ToString()))
                res.FanSpeed = -1;

            return res;
        }
    }

    public class AirConCount
    {
        public string Timestamp { get; set; }
        public string TotalCount { get; set; }
        public string RunningCount { get; set; }
        public string StoppingCount { get; set; }
        public string OfflineCount { get; set; }
        /* total air con number - all the time
         total running air con(timestamp<1h, and status is on) - all network
         total stopped air con(timestamp<1h, and status is off) - all network
         offline air con(error) - timestamp > 1h - count - all the network, all the time*/

        public AirConCount()
        {
            Timestamp = DateTime.Now.ToString();
            TotalCount = "0";
            RunningCount = "0";
            StoppingCount = "0";
            OfflineCount = "0";
        }
    }

    public class CountDuration
    {
        public string Count { get; set; }//count or power consumption in this duration
        public string Duration { get; set; }//can be hours, days, weeks, or months, years, we use  week1, week2, month1, month2, year 2021, year 2020
    }
    public class PowerStatictics
    {
        public string LocationName { get; set; }
        public string Timestamp { get; set; } //time of calculating this 
        public List<CountDuration> WeekCount;
        public List<CountDuration> MonthCount;
        public List<CountDuration> YearCount;

        public PowerStatictics()
        {
            LocationName = "NoName";
            Timestamp = DateTime.Now.ToString();
            WeekCount = new List<CountDuration>();
            MonthCount = new List<CountDuration>();
            YearCount = new List<CountDuration>();
        }
    }
}
