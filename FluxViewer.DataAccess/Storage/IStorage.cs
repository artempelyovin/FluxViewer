﻿using System;
using System.Collections.Generic;
using FluxViewer.DataAccess.Models;

namespace FluxViewer.DataAccess.Storage;

/// <summary>
/// Интерфейс взаимодействия с хранилищем показаний прибора.
/// В нём доступны методы чтения/записи показаний.
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Открыть хранилище.
    /// </summary>
    void Open();

    /// <summary>
    /// Закрыть хранилище.
    /// </summary>
    void Close();

    /// <summary>
    /// Добавление показания прибора в хранилище.
    /// </summary>
    /// <param name="data">Структура, описывающая 1 показание прибора</param>
    void WriteData(NewData data);

    /// <summary>
    /// Получение количества всех показаний, записанных за всё время.
    /// </summary>
    /// <returns>Количество показаний прибора</returns>
    public int GetDataCount();
    
    /// <summary>
    /// Получение количества показаний между двумя датами.
    /// </summary>
    /// <param name="beginDate">Дата начала, с которой считаются показания</param>
    /// <param name="endDate">Дата конца, по которую считаются показания</param>
    /// <returns>Количество показаний прибора</returns>
    public int GetDataCountBetweenTwoDates(DateTime beginDate, DateTime endDate, int? step = null);
    
    /// <summary>
    /// Получение количества всех батчей
    /// (батч - некоторый набор показаний прибора, гарантированно вмещающийся в оперативуню память)
    /// </summary>
    /// <returns>Количество показаний прибора</returns>
    public int GetBatchCount();
    
    
    /// <summary>
    /// Получение количества батчей между двумя датами.
    /// </summary>
    /// <param name="beginDate">Дата начала, с которой считаются батчи</param>
    /// <param name="endDate">Дата конца, по которую считаются батчи</param>
    /// <returns>Количество показаний прибора</returns>
    public int GetBatchCountBetweenTwoDates(DateTime beginDate, DateTime endDate);
    
    /// <summary>
    /// "Пакетное" получение показаний прибора между двумя датами из хранилища.
    /// </summary>
    /// <param name="beginDate">Дата начала, с которой следует искать показания</param>
    /// <param name="endDate">Дата конца, по которую следует искать показания</param>
    /// <param name="batchNumber">Номер пакета</param>
    /// <returns>Все показания прибора в текущий временной интервал</returns>
    public List<NewData> GetDataBatchBetweenTwoDates(DateTime beginDate, DateTime endDate, int batchNumber);
}