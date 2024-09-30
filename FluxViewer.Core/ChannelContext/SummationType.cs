using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChannelContext
{
    /// <summary>
    /// Суммирующий тип канала
    /// </summary>

    [Serializable]
    public class SummationType
    {
        private Double _baseKValue;
        public Double BaseKValue 
        { 
            get 
            {
                return _baseKValue;
            }
            set
            {
                this._baseKValue = value;

                ChannelContextHolder.Syncronize(this);
            }
        }

        /// <summary>
        /// Список пар значений где первое значение id канала, второе значение - его KValue
        /// </summary>
        private List<Tuple<int, Double>> _kValues;
        public List<Tuple<int, Double>> KValues
        {
            get
            {
                return _kValues;
            }
            set
            {
                this._kValues = value;

                ChannelContextHolder.Syncronize(this);
            }
        }

        public SummationType()
        {
            this.BaseKValue = 0;
            this.KValues = new List<Tuple<int, double>>();
        }

        public SummationType(Double baseKValue, List<Tuple<int, Double>> kValues)
        {
            this.BaseKValue = baseKValue;
            this.KValues = kValues;
        }

        /// <summary>
        /// Добавляет пару Tuple<Id, KValue>
        /// </summary>
        /// <param name="valuePair"></param>
        /// <exception cref="ArgumentNullException">Если передан null</exception>
        /// <exception cref="ArgumentException">Если уже есть пара с данным Id</exception>
        public void AddKValue(Tuple<int, Double> valuePair)
        {
            if (valuePair == null)
                throw new ArgumentNullException("Null argument present, expected Tuple<int, Double>");

            int id = valuePair.Item1;

            // Проверка если пара с данным id уже существует
            Tuple<int, Double> existingPair = this.KValues.Where(x => x.Item1 == id).FirstOrDefault();

            if (existingPair != null)
                throw new ArgumentException("Pair with such id already exists");

            this.KValues.Add(valuePair);

            ChannelContextHolder.Syncronize(this);
        }


        /// <summary>
        /// Удаляет пару по значению id канала
        /// </summary>
        /// <param name="id">id канала</param>
        public void RemoveKValueById(int id)
        {
            this.KValues.RemoveAll(x => x.Item1 == id);

            ChannelContextHolder.Syncronize(this);
        }
    }
}
