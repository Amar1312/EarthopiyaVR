using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;


#region FlightData
[Serializable]
public class FlightData
{
    public List<TicketData> ticketData;
}

[Serializable]
public class TicketData
{
    public string destinationCode;
    public string destinationName;
    public string destinationAirport;
    public string departTime;
    public string TimeDuration;
    public string stops;
    public string arrivalTime;
    public string adultPrice;
    public string discountPrice;
    public string FlightCode;
    public string GateCode;
}
#endregion

#region Login
[Serializable]
public class LoginResponce
{
    public bool status;
    public string message;
    public Data data;
}
[Serializable]
public class Data
{
    public User user;
    public string token;
}
[Serializable]
public class User
{
    public int id;
    public string email;
    public string firstname;
    public string lastname;
    public string dob;
    public string phone_no;
    public string profile_image_url;
    public List<string> passport;
}
#endregion

#region Profile
[Serializable]
public class ProfileResponce
{
    public bool status;
    public string message;
    public Profile_Data data;
}
[Serializable]
public class Profile_Data
{
    public Profile user;
}
[Serializable]
public class Profile
{
    public int id;
    public string email;
    public string firstname;
    public string lastname;
    public string dob;
    public string phone_no;
    public string profile_image_url;
    public List<string> passport;
    public string google_id;
    public string apple_id;
}

#endregion

#region Update Profile
[Serializable]
public class UpdateProfileResponce
{
    public bool status;
    public string message;
    public UpdateProfile_Data data;
}
[Serializable]
public class UpdateProfile_Data
{
    public UpdateProfile user;
}
[Serializable]
public class UpdateProfile
{
    public int id;
    public string email;
    public string firstname;
    public string lastname;
    public string dob;
    public string phone_no;
    public string profile_image_url;
    public List<string> passport;
}
#endregion

#region UpdatePassport
[Serializable]
public class Update_passport
{
    public List<string> passport;
}
[Serializable]
public class UpdatePassportResponce
{
    public bool status;
    public string message;
    public Update_passport data;
}
#endregion

#region Delete Account
[Serializable]
public class DeleteAccountResponce
{
    public bool status;
    public string message;
}
#endregion

#region Log Out
[Serializable]
public class LogOutResponce
{
    public bool status;
    public string message;
}
#endregion