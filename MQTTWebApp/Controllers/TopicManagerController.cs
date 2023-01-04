using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoAccess.DataAccess;
using MongoAccess.Model;
using MongoDB.Driver;
using MQTTTopicManager;

namespace MQTTWebApp.Controllers
{
    //[ApiController]
    // [Route("api/[controller]")]
    public class TopicManagerController : Controller
    {
        private readonly ILogger<TopicManagerController> _logger;
        private readonly DeviceMongoDriver<DeviceObject> _deviceMongoDriverDeviceObject;
        private readonly DeviceMongoDriver<TimeSeriesObject> _deviceMongoDriverTimeSeriesObject;
        private readonly ITopicManager _topicManager;


        public TopicManagerController(ILogger<TopicManagerController> logger, DeviceMongoDriver<DeviceObject> deviceMongoDriverDeviceObject, DeviceMongoDriver<TimeSeriesObject> deviceMongoDriverTimeSeriesObject, ITopicManager topicManager)
        {
            _logger = logger;
            _deviceMongoDriverDeviceObject = deviceMongoDriverDeviceObject;
            _deviceMongoDriverTimeSeriesObject = deviceMongoDriverTimeSeriesObject;
            _topicManager = topicManager;
            //  _topicManager.ConnectMqttClientAsync();
        }
        public IActionResult Dashboard()
        {
            return View();

            // return Json(new { mytest = DateTime.Now.ToString("h:mm:ss tt") });
        }
        public IActionResult AutoSelectDemo()
        {
            return View();
        }
        public IActionResult Charts()
        {
            return View();
        }

        #region APIs
        [HttpGet]
        public string CampusList()
        {
            // var campusList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Select(c => new { c.Campus }).Distinct().ToList();
            //  return JsonSerializer.Serialize(campusList.ToList());
            return _topicManager.GetAllCampusList();
        }

        /*  [HttpGet]
          public string BuildingList(string campus)
          {
              //  if (!String.IsNullOrEmpty(campus))
              //   _topicManager.SetSelecter(campus, 1);


              //   var buildingList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == campus).Select(c => new //{ c.Building }).Distinct().ToList();
              //   return JsonSerializer.Serialize(buildingList.ToList());
              return _topicManager.GetAllBuildingList(campus);
          }*/

        [HttpGet]
        public string MultipleBuildingList(string campusList)
        {
            //  if (!String.IsNullOrEmpty(campus))
            //   _topicManager.SetSelecter(campus, 1);                   

            //   var buildingList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == campus).Select(c => new //{ c.Building }).Distinct().ToList();
            //   return JsonSerializer.Serialize(buildingList.ToList());
            return _topicManager.GetMultipleSelectionBuildingList(campusList);
        }

        [HttpGet]
        public string MultipleSelectionConfigList(string input)
        {
            //  if (!String.IsNullOrEmpty(campus))
            //   _topicManager.SetSelecter(campus, 1);                   

            //   var buildingList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == campus).Select(c => new //{ c.Building }).Distinct().ToList();
            //   return JsonSerializer.Serialize(buildingList.ToList());
            return _topicManager.GetMultipleSelectionConfigList(input);
        }

        [HttpGet]
        public string MultipleRoomList(string buildingList)
        {
            //  if (!String.IsNullOrEmpty(campus))
            //   _topicManager.SetSelecter(campus, 1);                   

            //   var buildingList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == campus).Select(c => new //{ c.Building }).Distinct().ToList();
            //   return JsonSerializer.Serialize(buildingList.ToList());
            return _topicManager.GetMultipleSelectionRoomList(buildingList);
        }

        /*    [HttpGet]
            public string RoomList(string building)
            {
                // if (!String.IsNullOrEmpty(building))
                //     _topicManager.SetSelecter(building, 2); 

                //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
                //   return JsonSerializer.Serialize(roomList.ToList());
                return _topicManager.GetAllRoomList(building);
            }
            [HttpGet]
            public async Task<string> GetRoomData(string room)
            {
                // if (!String.IsNullOrEmpty(building))
                //     _topicManager.SetSelecter(building, 2); 

                //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
                //   return JsonSerializer.Serialize(roomList.ToList());
                return await _topicManager.GetRoomData(room);
            }*/

        [HttpGet]
        public async Task<string> MultipleRoomListData(string roomList)
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.MultipleRoomListData(roomList);
        }
        [HttpGet]
        public string GetHolidayList()
        {
            return _topicManager.GetHolidayList();
        }

        [HttpGet]
        public async Task<string> GetWarningList()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetWarningList();
        }


        [HttpGet]
        public async Task<string> GetAddSensorJson()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetAddSensorJson();
        }

        [HttpGet]
        public async Task<string> GetChartsList()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetChartsList();
        }

        [HttpGet]
        public async Task<string> GetSensorStatus()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetSensorStatus();
        }


        [HttpGet]
        public async Task<string> GetCountCards()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetCountCards();
        }


        [HttpGet]
        public async Task<string> GetSensorData()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetSensorData();
        }

        [HttpGet]
        public async Task<string> GetPowerStatistics()
        {
            // if (!String.IsNullOrEmpty(building))
            //     _topicManager.SetSelecter(building, 2); 

            //  var roomList = _deviceMongoDriverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == _topicManager.GetSelecter(1) && c.Building == _topicManager.GetSelecter(2)).Select(c => new { c.Room }).Distinct().ToList();
            //   return JsonSerializer.Serialize(roomList.ToList());
            return await _topicManager.GetPowerStatistics();
        }


        [HttpGet]
        public async Task InitializeSensor(string sensorId)
        {
            await _topicManager.InitializeSensor(sensorId);
        }


        [HttpPost]
        /* public async Task PublishMessage(string topic, TimeSeriesObjectForJsonSent payload = null)
         {
             if (payload != null)
             {
                 await _topicManager.PublishMessageAsync(topic, JsonSerializer.Serialize(payload));
             }
             // payload.Timestamp = DateTime.Now;
        //
              TimeSeriesObjectForJsonMV newMsg = new TimeSeriesObjectForJsonMV();
              newMsg.TopicName = payload.TopicName;
              newMsg.PowerOnOff = payload.PowerOnOff.ToString();
              newMsg.Timestamp = DateTime.Now.ToLocalTime().ToString();
              newMsg.Temperature = payload.Temperature.ToString();
              newMsg.FanSpeed = payload.FanSpeed.ToString();
              newMsg.SensorId = payload.SensorId;
              newMsg.Note = payload.Note; //

             // var oldPayload = new { Temperature = "25",FanSpeed = "5", Timestamp = DateTime.Now, PowerOnOdd = false};
             // await topicManager.PublishMessageAsync("Bundoora/Mitsobishi/Research/CTI/East/1/5/1/s1", JsonSerializer.Serialize(payload));
             // await topicManager.PublishMessageAsync("Melbourne/Fujitsu/Business/B1/South/2/3/2/s2", JsonSerializer.Serialize(payload));
             // await topicManager.PublishMessageAsync("Sensor/0002x", JsonSerializer.Serialize(payload));
             //await topicManager.PublishMessageAsync("Sensor/s1", "{\"Temperature\":\"35\",\"FanSpeed\":\"4\"}");        
         }*/
        [HttpPost]
        public async Task PublishSimpleMessage(string temperature, string fanspeed, string poweronoff, string mode, string cmd, string sensorid, string schedule, string resettimer)
        {
            await _topicManager.PublishSimpleMessage(temperature, fanspeed, poweronoff, mode, cmd, sensorid, schedule, resettimer);
        }
        [HttpPost]
        public async Task UpdateSensorID(string id, string sensorid, string vendor, string campus, string building, string room, string wakeuptimer, bool isDelete)
        {
            await _topicManager.UpdateSensorID(id, sensorid, vendor, campus, building, room, wakeuptimer, isDelete);
        }
        [HttpPost]
        public async Task OperateConfigureTable(string jasondata, bool isDelete)
        {
            await _topicManager.OperateConfigureTable(jasondata, isDelete);
        }

        #endregion
    }
}
