using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace FluxViewer.Core.ChannelContext
{

    /// <summary>
    /// Класс контейнер (singleton).
    /// Отвечает за хранение информации по каналам,
    /// а также сохранение и загрузку данных из файла-хранилища.
    /// 
    /// Загружает объект из файла при первом доступе к экземпляру
    /// Сохраняет объект в файл при каждом изменении данных
    /// </summary>
    
    [Serializable]
    public class ChannelContextHolder
    {
        static private ChannelContextHolder _instance = null;


        // Путь к файлу где хранятся данные по каналам
        static private String _filePath = "channel-context.bin";

        private List<Channel> channels;

        private List<int> generalWindowChannelIds;

        // Список всех каналов
        public List<Channel> Channels {
            get 
            {
                // Возвращаю КОПИЮ чтобы не было возможности пропихнуть канал без синхронизации с файлом
                return this.channels.ToList();
            }
            private set 
            {
                this.channels = value;
            }
        }

        // Список id каналов, которые отрисовываются на общем окне
        // Кнопка "Общее окно" из прошлой версии
        public List<int> GeneralWindowChannelIds {
            get
            {
                // Возвращаю КОПИЮ чтобы не было возможности пропихнуть без синхронизации с файлом
                return this.generalWindowChannelIds.ToList();
            }
            private set
            {
                this.generalWindowChannelIds = value;
            }
        }

        static public String FilePath
        {
            get { return ChannelContextHolder._filePath; }
            set { ChannelContextHolder._filePath = value; }
        }

        private ChannelContextHolder()
        {
            this.Channels = new List<Channel>();
            this.generalWindowChannelIds = new List<int>();
        }

        private ChannelContextHolder(List<Channel> Channels, List<int> GeneralWindowChannelIds)
        {
            this.channels = Channels;
            this.generalWindowChannelIds = GeneralWindowChannelIds;
        }


        /// <summary>
        /// Метод для получение единственной копии
        /// класса ChannelContextHolder. Так как singleton
        /// </summary>
        /// 
        /// <returns>
        /// Объект типа ChannelContextHolder
        /// </returns>
        static public ChannelContextHolder GetInstance()
        {
            if (ChannelContextHolder._instance == null)
                ChannelContextHolder._instance = ChannelContextHolder.CreateInstance();

            return _instance;
        }

        static private ChannelContextHolder CreateInstance()
        {
            if (File.Exists(ChannelContextHolder._filePath))
            {
                ChannelContextHolder instance = ChannelContextHolder.LoadFromFile();
                if (instance != null)
                    return instance;
            }

            // Если не удалось найти файл или ошибка при десериализации 
            return new ChannelContextHolder();
        }

        static private ChannelContextHolder GetSavableCopy()
        {
            if (ChannelContextHolder._instance == null)
                throw new InvalidOperationException("Instance is null");

            List<Channel> savableChannels = _instance.channels.Where(x => x.Save == true).ToList();

            List<int> savableChannelIds = savableChannels.Select(x => x.Id).ToList();

            // Список Id каналов которые отображаются в общем окне
            List<int> savableGeneralWindowChannelIds = _instance.generalWindowChannelIds.Except(
                _instance.generalWindowChannelIds.Except(savableChannelIds).ToList()
                ).ToList();

            return new ChannelContextHolder(savableChannels, savableGeneralWindowChannelIds);
        }

        /// <summary>
        /// Пытается создать объект ChannelContextHolder из файла 
        /// и вернуть его
        /// </summary>
        /// <returns> ChannelContextHolder если удалось десериализовать иначе null </returns>
        static private ChannelContextHolder LoadFromFile()
        {
            try
            {
                ChannelContextHolder instance;
                using (FileStream fs = new FileStream(ChannelContextHolder.FilePath, FileMode.OpenOrCreate))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    instance = (ChannelContextHolder)formatter.Deserialize(fs);
                }

                return instance;
            }
            catch (Exception e)
            {
                return null;
            } 
        }

        static private void SaveToFile()
        {
            if (ChannelContextHolder._instance == null)
                throw new InvalidOperationException("Saving before init");

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                using (FileStream fs = new FileStream(ChannelContextHolder.FilePath, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, GetSavableCopy());
                }
            }
            catch (Exception e)
            { /* а Че */ }
        }

        /// <summary>
        /// Метод для синхронизации при модификации объектов типа Channel и IChannelType
        /// Вызывается каждый раз при вызове сеттера в соответствующих классах
        /// </summary>
        /// <param name="obj">Объект у которого был вызван </param>
        static public void Syncronize(Object obj)
        {
            if (obj == null || ChannelContextHolder._instance == null)
                return;

            Channel changedObj = null;

            if (obj.GetType() == typeof(Channel))
                changedObj = GetInstance().Channels.FirstOrDefault(x => x == obj);

            if (obj.GetType() == typeof(IChannelType))
                changedObj = GetInstance().Channels.FirstOrDefault(x => x.ChannelType == obj);

            if (changedObj != null)
                SaveToFile();
        }

        /// <summary>
        /// Добавляет канал в список всех каналов
        /// Добавляет канал в список каналов общего окна
        /// </summary>
        /// <param name="channel">Канал который добавляется</param>
        /// <param name="addToGeneralWindow">Если также нужно добавить канал в общее окно</param>
        /// <exception cref="ArgumentException">В случае если уже существует канал с его Id</exception>
        public void AddChannel(Channel channel, bool addToGeneralWindow = true)
        {
            // Если канал с данным Id уже существует
            if (this.channels.FirstOrDefault(x => x.Id == channel.Id) != null 
                || this.generalWindowChannelIds.Contains(channel.Id))
                throw new ArgumentException("Channel with such id already exists");

            this.channels.Add(channel);
            if (addToGeneralWindow)
                this.generalWindowChannelIds.Add(channel.Id);

            ChannelContextHolder.SaveToFile();
        }

        /// <summary>
        /// Возвращает список каналов для общего окна
        /// Кнопка "Общее окно"
        /// </summary>
        /// <returns>List<Channel></returns>
        public List<Channel> GetGeneralWindowChannels()
        {
            return this.channels.Where(x => this.generalWindowChannelIds.Contains(x.Id)).ToList();
        }

        /// <summary>
        /// Возвращает канал с данным Id
        /// </summary>
        /// <param name="id">Id канала</param>
        /// <returns>Channel или null</returns>
        public Channel GetChannelById(int id)
        {
            return this.channels.FirstOrDefault(x => x.Id == id);
        }


        /// <summary>
        /// Удаляет канал из списка каналов
        /// Удаляет канал из списка каналов для общего окна
        /// </summary>
        /// <param name="id"></param>
        public void RemoveChannelById(int id)
        {
            this.channels.RemoveAll(x => x.Id == id);
            this.generalWindowChannelIds.Remove(id);

            ChannelContextHolder.SaveToFile();
        }

        /// <summary>
        /// Удаляет канал из вкладки "Общее окно" по его Id
        /// 
        /// </summary>
        /// <param name="id">id канала</param>
        public void RemoveChannelFromGeneralWindowById(int id)
        {
            this.generalWindowChannelIds.Remove(id);

            ChannelContextHolder.SaveToFile();
        }
    }
}