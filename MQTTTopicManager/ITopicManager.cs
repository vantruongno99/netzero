using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace MQTTTopicManager
{
    public interface ITopicManager
    {
        public Task ConnectMqttClientAsync();
        public Task InitializeSensor(string sensorId);
        public Task PublishMessageAsync(string topic, string payload);
        public Task TopicManagerInit(IServiceProvider serviceProvider);
        public string GetAllCampusList();
        //   public string GetAllBuildingList(string campus);
        //   public string GetAllRoomList(string building);
        //   public Task<string> GetRoomData(string room);
        public Task<string> MultipleRoomListData(string roomList);
        public string GetHolidayList();

        public Task<string> GetAddSensorJson();
        public Task UpdateSensorID(string id, string sensorid, string vendor, string campus, string building, string room, string wakeuptimer, bool isDelete);
        public Task SubscribeUnSubscribeAsync(string SensorId, string wakeuptimer, bool isSubscribe);
        public Task<string> GetWarningList();
        public Task<string> GetChartsList();
        public Task<string> GetSensorStatus();
        public Task<string> GetCountCards();

        public Task<string> GetSensorData();

        public Task<string> GetSensorDataCustom(int date);

        public Task<string> GetPowerStatistics();
        public string GetMultipleSelectionBuildingList(string campusList);
        public string GetMultipleSelectionRoomList(string campusList);
        public Task PublishSimpleMessage(string temperature, string fanspeed, string OnOff, string note, string topic, string sensorid, string schedule, string resettimer);
        public string GetMultipleSelectionConfigList(string input);

        public Task OperateConfigureTable(string jasondata, bool isDelete);
    }
}
