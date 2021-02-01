using System;
using System.Collections.Generic;
using StorageLib;


namespace MusicPlayer  {


  public partial class Location: IStorageItem<Location> {


    #region Properties
    //      ----------

    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// Called once the constructor has filled all the properties
    /// </summary>
    //partial void onConstruct() {
    //}


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    //partial void onCloned(Location clone) {
    //}


    /// <summary>
    /// Called once the CSV-constructor who reads the data from a CSV file has filled all the properties
    /// </summary>
    //partial void onCsvConstruct() {
    //}


    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Called before {ClassName}.Store() gets executed
    /// </summary>
    //partial void onStoring(ref bool isCancelled) {
    //}


    /// <summary>
    /// Called after Location.Store() is executed
    /// </summary>
    //partial void onStored() {
    //}


    /// <summary>
    /// Called before Location gets written to a CSV file
    /// </summary>
    //partial void onCsvWrite() {
    //}


    /// <summary>
    /// Called after all properties of Location are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdating(string path, string name, ref bool isCancelled){
   //}


    /// <summary>
    /// Called after all properties of Location are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdated(Location old) {
    //}


    /// <summary>
    /// Called after an update for Location is read from a CSV file
    /// </summary>
    //partial void onCsvUpdate() {
    //}


    /// <summary>
    /// Called after Location.Release() got executed
    /// </summary>
    //partial void onReleased() {
    //}


    /// <summary>
    /// Called after 'new Location()' transaction is rolled back
    /// </summary>
    //partial void onRollbackItemNew() {
    //}


    /// <summary>
    /// Called after Location.Store() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemStored() {
    //}


    /// <summary>
    /// Called after Location.Update() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemUpdated(Location oldLocation) {
    //}


    /// <summary>
    /// Called after Location.Release() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemRelease() {
    //}


    /// <summary>
    /// Updates returnString with additional info for a short description.
    /// </summary>
    //partial void onToShortString(ref string returnString) {
    //}


    /// <summary>
    /// Updates returnString with additional info for a short description.
    /// </summary>
    //partial void onToString(ref string returnString) {
    //}
    #endregion
  }
}
