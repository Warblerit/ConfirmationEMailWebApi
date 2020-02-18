ALTER PROCEDURE [dbo].[SP_ConfirmationEMail_Help](@Id BIGINT,@Str NVARCHAR(MAX))                    
AS                              
BEGIN                              
SET NOCOUNT ON                              
SET ANSI_WARNINGS OFF                              
DECLARE @PName NVARCHAR(100), @MobileNo NVARCHAR(MAX), @SecurityPolicy NVARCHAR(1000), @CancelationPolicy NVARCHAR(1000);                              
DECLARE @BookingPropertyId BIGINT, @ClientId1 BIGINT, @PropertyEmail NVARCHAR(100) = '', @CLogo NVARCHAR(1000) = '', @CLogoAlt NVARCHAR(100) = '', @Cnt INT = 0;                              
--                              
SET @CLogo = (SELECT TOP 1 Logo FROM WRBHBCompanyMaster WHERE IsActive = 1 AND IsDeleted = 0);                              
SET @CLogoAlt = 'Staysimplyfied';                              
/*-- PART OF CLIENT NAME */         
DECLARE @ClientName NVARCHAR(100),@ClientName1 NVARCHAR(100);                              
SELECT @ClientName = dbo.TRIM(ClientName) FROM WRBHBClientManagement WHERE Id = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id);                              
                    
CREATE TABLE #QAZXSW(Id INT,Name NVARCHAR(100));                              
INSERT INTO #QAZXSW(Id,Name)                              
SELECT * FROM dbo.Split(@ClientName,' ');                              
                    
SET @ClientName1 = (SELECT TOP 1 Name FROM #QAZXSW);                              
/*-- CLIENT LOGO IN M G H */       DECLARE @cltlogoMGH nvarchar(1000), @cltaltMGH nvarchar(100);                              
SELECT @cltlogoMGH = ISNULL(ClientLogo,''),@cltaltMGH = ISNULL(ClientName,'') FROM WRBHBClientManagement                              
WHERE Id = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id);                              
                    
DECLARE @Taxes NVARCHAR(100),@TypeofPtyy NVARCHAR(100),@TXADED NVARCHAR(100),@BaseTariff NVARCHAR(100);                              
SELECT TOP 1 @TXADED = ISNULL(TaxAdded,'T'), @TypeofPtyy = PropertyType,                               
@BaseTariff = CASE WHEN BaseTariff = 0 THEN  CAST(BaseTariff AS VARCHAR) ELSE CAST((BaseTariff) AS VARCHAR) END                            
FROM WRBHBBookingProperty WHERE Id IN (SELECT TOP 1 BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId = @Id);                              
                    
DECLARE @Stay NVARCHAR(100),@Uniglobe NVARCHAR(100);                              
SET @Stay = 'stay@hummingbirdindia.com';                       
SET @Uniglobe = 'homestay@uniglobeatb.co.in';                              
                    
DECLARE @MailStr NVARCHAR(1000)='';                              
                    
DECLARE @NeedhelpbookingText NVARCHAR(MAX) = '';                              
IF EXISTS(SELECT NULL FROM WRBHBClientSMTP WHERE IsActive = 1 AND IsDeleted = 0 AND ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id))                              
BEGIN                              
SET @NeedhelpbookingText = ' - stay@hummingbirdindia.com';                              
END                              
ELSE                              
BEGIN                              
SET @NeedhelpbookingText = ' - stay@hummingbirdindia.com';                              
END  

DECLARE @Action NVARCHAR(MAX);
SET @Action = (SELECT CASE WHEN BookingLevel='Room' THEN 'RoomBookingConfirmed' ELSE 'BedBookingConfirmed' END FROM WRBHBBooking WHERE Id = @Id)
   
IF @Action = 'RoomBookingConfirmed'                              
 BEGIN                    
  DECLARE @PayMode NVARCHAR(100), @BookingPropertyTableId BIGINT;                              
  SELECT TOP 1 @PayMode = TariffPaymentMode, @BookingPropertyTableId = BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId = @Id;                              
  /* -- Dataset Table 0 */                    
  CREATE TABLE #FFF(BookingId BIGINT,RoomId BIGINT,ChkInDt NVARCHAR(100),                              
  ChkOutDt NVARCHAR(100),Tariff DECIMAL(27,2),Occupancy NVARCHAR(100),                              
  TariffPaymentMode NVARCHAR(100),ServicePaymentMode NVARCHAR(100),                              
  RoomCaptured INT,RoomNo NVARCHAR(100),BookingLevel NVARCHAR(MAX),ChkInDt1 NVARCHAR(100),ChkOutDt1 NVARCHAR(100),ExpectChkInTime NVARCHAR(100));                              
  INSERT INTO #FFF(BookingId,RoomId,ChkInDt,ChkOutDt,Tariff,Occupancy,                              
  TariffPaymentMode,ServicePaymentMode,RoomCaptured,RoomNo,BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime)                              
  SELECT BG.BookingId,BG.RoomId,                              
  REPLACE(CONVERT(VARCHAR(11),BG.ChkInDt, 106), ' ', '-') +' / '+                               
  LEFT(BG.ExpectChkInTime, 5)+' '+BG.AMPM,                              
  REPLACE(CONVERT(VARCHAR(11), BG.ChkOutDt, 106), ' ', '-'),                              
  BG.Tariff,BG.Occupancy,                              
  CASE WHEN BG.TariffPaymentMode='Direct' THEN 'Direct<br>(Cash/Card)'                              
       WHEN BG.TariffPaymentMode = 'Bill to Client' THEN 'Bill to '+@ClientName1                        
       ELSE BG.TariffPaymentMode END AS TariffPaymentMode,                              
  CASE WHEN BG.ServicePaymentMode='Direct' THEN 'Direct<br>(Cash/Card)'                              
       WHEN BG.ServicePaymentMode = 'Bill to Client' THEN 'Bill to '+@ClientName1                        
       ELSE BG.ServicePaymentMode END AS ServicePaymentMode,BG.RoomCaptured,                              
       BG.RoomType,'Room',convert(varchar, BG.ChkInDt, 107),
	   convert(varchar, BG.ChkOutDt, 107),LEFT(BG.ExpectChkInTime, 5)+' '+BG.AMPM                               
  FROM WRBHBBookingPropertyAssingedGuest BG                              
  WHERE BG.IsActive=1 AND BG.IsDeleted=0 AND BG.BookingId=@Id AND                              
  ISNULL(BG.RoomShiftingFlag,0) = 0                               
GROUP BY BG.BookingId,BG.RoomId,BG.ChkInDt,BG.ExpectChkInTime,BG.AMPM,                              
  BG.ChkOutDt,BG.Tariff,BG.Occupancy,BG.TariffPaymentMode,                              
  BG.ServicePaymentMode,BG.RoomCaptured,BG.RoomType;                              
  CREATE TABLE #QAZ(Name NVARCHAR(100),ChkInDt NVARCHAR(100),                              
  ChkOutDt NVARCHAR(100),Tariff DECIMAL(27,2),Occupancy NVARCHAR(100),                              
  TariffPaymentMode NVARCHAR(100),ServicePaymentMode NVARCHAR(100),                              
  RoomNo NVARCHAR(100),BookingLevel NVARCHAR(100),ChkInDt1 NVARCHAR(100),ChkOutDt1 NVARCHAR(100),ExpectChkInTime NVARCHAR(100),
  NameWthTitle NVARCHAR(100));                              
  INSERT INTO #QAZ(Name,ChkInDt,ChkOutDt,Tariff,Occupancy,TariffPaymentMode,                              
  ServicePaymentMode,RoomNo,BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime,NameWthTitle)                              
                    
  SELECT STUFF((SELECT ', '+BA.Title+'. '+BA.FirstName                              
FROM WRBHBBookingPropertyAssingedGuest BA                          
  WHERE BA.BookingId=B.BookingId AND BA.RoomCaptured=B.RoomCaptured AND BA.Isactive=1 AND BA.IsDeleted=0 AND                             
  ISNULL(BA.RoomShiftingFlag,0) = 0                              
  FOR XML PATH('')),1,1,'') AS Name,B.ChkInDt,B.ChkOutDt,                              
  B.Tariff,B.Occupancy,B.TariffPaymentMode,B.ServicePaymentMode,B.RoomNo,B.BookingLevel,B.ChkInDt1,B.ChkOutDt1,B.ExpectChkInTime,
  STUFF((SELECT ', '+BA.Title+'. '+BA.FirstName                               
FROM WRBHBBookingPropertyAssingedGuest BA                          
  WHERE BA.BookingId=B.BookingId AND BA.RoomCaptured=B.RoomCaptured AND BA.Isactive=1 AND BA.IsDeleted=0 AND                             
  ISNULL(BA.RoomShiftingFlag,0) = 0                              
  FOR XML PATH('')),1,1,'') AS NameWthTitle                              
  FROM #FFF AS B;                       
                      
   SELECT TOP 1 @BookingPropertyId=BookingPropertyId                               
   FROM WRBHBBookingPropertyAssingedGuest                              
   WHERE IsActive=1 AND IsDeleted=0 AND BookingId=@Id;                     
                      
   DECLARE @Facilities NVARCHAR(500)=(SELECT TOP 1 Inclusions FROM WRBHBBookingProperty BP                    
   JOIN WRBHBBookingPropertyAssingedGuest  BG ON BP.Id=BG.BookingPropertyTableId                                
   WHERE BG.IsActive=1 AND BG.IsDeleted=0 AND BG.BookingId=@Id)                       
                      
   DECLARE @PType NVARCHAR(500)=(SELECT TOP 1 BP.PropertyType FROM WRBHBBookingProperty BP                    
   JOIN WRBHBBookingPropertyAssingedGuest  BG ON BP.Id=BG.BookingPropertyTableId                                
   WHERE BG.IsActive=1 AND BG.IsDeleted=0 AND BG.BookingId=@Id)                     
                         
   DECLARE @BPId1 BIGINT=(SELECT TOP 1 BP.Id FROM WRBHBBookingProperty BP                    
   JOIN WRBHBBookingPropertyAssingedGuest  BG ON BP.Id=BG.BookingPropertyTableId                                
   WHERE BG.IsActive=1 AND BG.IsDeleted=0 AND BG.BookingId=@Id)                         
                    
   DECLARE @TAX BIT,@LTAgreed DECIMAL(27,2),@STAgreed DECIMAL(27,2),@LTRack DECIMAL(27,2)                    
                    
   SELECT @TAX=TaxInclusive,@LTAgreed=LTAgreed,@STAgreed=STAgreed,@LTRack=LTRack FROM WRBHBBookingProperty WHERE Id=@BPId1                        
                               
  /* -- ICICI Lombard General Insurance Company Ltd NOT SHOWN START TARIFF (29 sep 2015) */                    
  IF EXISTS(SELECT ClientId FROM WRBHBBooking WHERE Id = @Id AND ClientId = 2168)                              
   BEGIN                              
    SELECT Name,ChkInDt,ChkOutDt,'Sharing' AS Tariff,Occupancy,TariffPaymentMode,ServicePaymentMode,RoomNo,BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime                   
    FROM #QAZ;                    
   END                              
  ELSE                              
   BEGIN                        
    IF @PType='CPP' AND @TAX=0 AND  @LTAgreed=0 AND @STAgreed=0 AND @LTRack=0                    
     BEGIN                             
      SELECT Name,ChkInDt,ChkOutDt,CAST(Tariff AS VARCHAR)+' /-<br> + Taxes',Occupancy,TariffPaymentMode,ServicePaymentMode,RoomNo,
	  BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime FROM #QAZ;                    
  END                    
 ELSE IF @PType = 'BOK'                    
     BEGIN                             
      SELECT Name, ChkInDt, ChkOutDt, CAST(Tariff AS VARCHAR)+' /-<br> + Taxes', Occupancy, TariffPaymentMode, ServicePaymentMode, RoomNo,
	  BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime FROM #QAZ;               
  END                    
 ELSE                    
  BEGIN                    
   SELECT Name,ChkInDt,ChkOutDt,                    
   /*--CASE WHEN Tariff != 0 AND @TXADED = 'N' THEN CAST(Tariff AS VARCHAR)+'/-<br>(Nett Tariff)'*/                    
   CASE WHEN Tariff != 0 AND @TXADED = 'N' THEN CAST(Tariff AS VARCHAR)+'/-<br>(Nett Tariff)'                    
   ELSE 
    CASE WHEN Tariff >= 500 THEN CAST(Tariff AS VARCHAR)+'/-' + '<br>  + Taxes' ELSE
    CAST(Tariff AS VARCHAR)+'/-' END 
   
   END, Occupancy,TariffPaymentMode,ServicePaymentMode,RoomNo,
   BookingLevel,ChkInDt1,ChkOutDt1,ExpectChkInTime FROM #QAZ;                    
  END                          
   END                    
                                
  IF @TXADED = 'N'                              
   BEGIN                              
    /* --SET @Taxes = 'Including Tax. [Base Tariff : '+@BaseTariff+' + Taxes]'; */                    
  SET @Taxes = 'Tariff details. [Base Tariff : '+@BaseTariff+' + Taxes]';                               END                              
  IF @TXADED = 'T'                              
   BEGIN                              
    SET @Taxes = 'Taxes as applicable. [Base Tariff :'+@BaseTariff+' + Taxes]';                              
   END                                
  /* -- dataset table 1 */                    
  DECLARE @PrType NVARCHAR(100), @GType NVARCHAR(100), @Facility NVARCHAR(100), @BOK_Phone NVARCHAR(100);                    
  SELECT @PrType = BP.PropertyType, @GType = BP.GetType, @Facility = BP.Inclusions,                
  @BOK_Phone = (CASE WHEN ISNULL(BP.AvailabilityResponseReferenceKey, '') = '' THEN 'NA' ELSE BP.AvailabilityResponseReferenceKey END)                  
  FROM WRBHBBookingProperty BP                            
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON BP.BookingId = BG.BookingId AND BP.Id = BG.BookingPropertyTableId AND BP.PropertyId = BG.BookingPropertyId                            
  WHERE BG.BookingId = @Id;                            
                      
  IF @PrType = 'ExP' AND @GType = 'API'                            
  BEGIN                          
  SELECT Propertaddress + ',' + Localityarea + ',' + City + ',' + State + ' - ' + Postal AS ADDRESS, Phone, BP.Directions, '' AS BookingPolicy, CancelPolicy, PropertyName,                            
  'External Property' AS Category, ISNULL('PM', '') AS CheckOutType, ISNULL(12, '') AS CheckIn, ISNULL('PM','') AS CheckInType, ISNULL(12,'') AS CheckOut,                            
  'External Property' AS PropertyType, ISNULL(@Facilities,'') AS Facility,                             
  REPLACE(PropertyName, ' ', '+')+'/@'+REPLACE(LatitudeLongitude, ' ', '')+',15z' AS Map,'' AS ExtraNote                            
  FROM dbo.WRBHBTreeboProperty BP                            
  WHERE BP.Id = @BookingPropertyId AND BP.IsActive = 1 AND BP.IsDeleted = 0;                     
  END                              
  ELSE IF @PrType = 'CTP'                            
  BEGIN                            
  SELECT Errorcode AS ADDRESS, Phone, '' AS Directions, @SecurityPolicy AS BookingPolicy,                               
  @CancelationPolicy AS CancelPolicy, PropertyName, 'Clear Trip' Category,                              
  '' AS CheckOutType, '' AS CheckIn, '' AS CheckInType, '' AS CheckOut, 'ClearTrip Property' AS PropertyType, Inclusions AS Facility,                              
  PropertyName + '/@' + RatePlanCode + ',15z' AS Map, '' AS ExtraNote                               
  FROM dbo.WRBHBBookingProperty bp where BP.Id = @BookingPropertyTableId;                              
  END                    
  ELSE IF @PrType = 'TBO'                            
  BEGIN                            
  SELECT Locality + ', ' + City + ', '+ Statee +', '+ Country + ', '+ Pincode AS ADDRESS, '' AS Phone, '' AS Directions, @SecurityPolicy AS BookingPolicy,                    
  CancellationPolicy AS CancelPolicy, HotelName AS PropertyName, 'Treebo' AS Category,                    
  '' AS CheckOutType, CheckInTime AS CheckIn, '' AS CheckInType, CheckOutTime AS CheckOut, 'Treebo Property' AS PropertyType, @Facility AS Facility,                    
  HotelName + '/@' + Latitude + ',' + Longitude + ',15z' AS Map, '' AS ExtraNote                    
  FROM dbo.WRBHBTreeboHotels                    
  WHERE Id = @BookingPropertyId;                              
  END                
  ELSE IF @PrType = 'BOK'                            
  BEGIN                    
  SELECT address +', '+ postel_code AS ADDRESS, @BOK_Phone AS Phone, hotel_description AS Directions, @SecurityPolicy AS BookingPolicy,                    
  '' AS CancelPolicy, hotel_name AS PropertyName, 'Booking.com' AS Category,                    
  '' AS CheckOutType, CONVERT(VARCHAR(15), CAST(checkintime AS TIME), 100) AS CheckIn,                    
  '' AS CheckInType, CONVERT(VARCHAR(15), CAST(checkouttime AS TIME), 100) AS CheckOut, 'Booking.com' AS PropertyType, @Facility AS Facility,                    
  hotel_name + '/@' + Latitude + ',' + Longitude + ',15z' AS Map, '' AS ExtraNote                  
  FROM dbo.Wrbhbbooking_com_hotels                    
  WHERE hotel_id = @BookingPropertyId;                            
  END                            
  ELSE IF @PrType = 'OYO'                            
  BEGIN                            
                      
  DECLARE @CheckInType VARCHAR(100) = '';                            
  SELECT TOP 1 @CheckInType = D.policies FROM WRBHBOYORooms H                            
  LEFT OUTER JOIN WRBHBOYOPolicies D WITH(NOLOCK)ON H.HotelId = D.HotelId                            
  WHERE H.Id = @BookingPropertyId AND D.policies LIKE '%Check in%'                            
  ORDER BY D.Id;                            
                      
  DECLARE @CheckOutType VARCHAR(100) = '';                            
  SELECT TOP 1 @CheckOutType = D.policies FROM WRBHBOYORooms H                            
  LEFT OUTER JOIN WRBHBOYOPolicies D WITH(NOLOCK)ON H.HotelId = D.HotelId                            
  WHERE H.Id = @BookingPropertyId AND D.policies LIKE '%Check out%'                            
  ORDER BY D.Id;                            
                      
  SELECT Addresss, '' AS Phone, Descriptionn AS Directions, '' AS BookingPolicy, '' AS CancelPolicy, HotelName AS PropertyName, 'OYO Property' AS Category,                            
  @CheckOutType AS CheckOutType, '' AS CheckIn, @CheckInType AS CheckInType, '' AS CheckOut, 'OYO' AS PropertyType, @Facilities AS Facility,                            
  HotelName + '/@' + (Latitude+','+Longitude) + ',15z' AS Map, '' AS ExtraNote                            
  FROM WRBHBOYORooms                            
  WHERE Id = @BookingPropertyId;                            
  END                    
  ELSE                    
  BEGIN                    
  SELECT Propertaddress+','+L.Locality+','+C.CityName+','+S.StateName+' - '+Postal AS ADDRESS,                              
  BP.Phone,BP.Directions,BookingPolicy,CancelPolicy,bp.PropertyName,                              
  CASE WHEN BP.Category IN ('Internal Property','External Property') THEN 'Note Removed' ELSE BP.Category END,                          
  ISNULL(BP.CheckOutType,'') CheckOutType,ISNULL(CheckIn,'') CheckIn,                           
  ISNULL(CheckInType,'') CheckInType,ISNULL(CheckOut,'') CheckOut,T.PropertyType,ISNULL(@Facilities,'') Facility,                    
 REPLACE(PropertyName, ' ', '+')+'/@'+REPLACE(LatitudeLongitude, ' ', '')+',15z' AS Map,BP.ExtraNote                                
  FROM dbo.WRBHBProperty BP                              
  LEFT OUTER JOIN WRBHBLocality L WITH(NOLOCK) ON L.Id=BP.LocalityId                              
  LEFT OUTER JOIN WRBHBCity C WITH(NOLOCK) ON L.CityId=C.Id                              
  LEFT OUTER JOIN WRBHBState S WITH(NOLOCK) ON S.Id=C.StateId                              
  LEFT OUTER JOIN WRBHBPropertyType T WITH(NOLOCK) ON T.Id=BP.PropertyType                              
  WHERE BP.Id=@BookingPropertyId  AND BP.IsActive=1 AND BP.IsDeleted=0;                     
  END              
                               
  /*-- dataset table 2 */                    
  IF EXISTS(SELECT NULL FROM WRBHBBooking WHERE Id = @Id AND                          
  ISNULL(HBStay,'') = 'StayCorporateHB')                      
   BEGIN                              
    SELECT ISNULL(ClientLogo,'') AS ClientLogo,C.ClientName,                              
    B.BookingCode,U.FirstName,U.Email,ISNULL(U.Mobile,''),B.ClientBookerName,                              
    /*--REPLACE(CONVERT(VARCHAR(11), B.CreatedDate, 106), ' ', '-'),*/                    
    REPLACE(CONVERT(VARCHAR(11), B.BookedDt, 106), ' ', '-'),                              
    B.SpecialRequirements,B.ClientBookerEmail,B.ExtraCCEmail,B.ClientId,B.RowId,B.Client_RequestNo,ISNULL(C.SupportCPhoneNo,'') AS Deskno,
	ISNULL(B.PropertyRefNo,'') AS PropertyRefNo,
	ISNULL(C.SupportEmail,'stay@hummingbirdindia.com') AS SupportEmail	                             
    FROM WRBHBBooking B                              
    LEFT OUTER JOIN WRBHBClientManagement C WITH(NOLOCK) ON  C.Id=B.ClientId                              
    LEFT OUTER JOIN WrbhbTravelDesk U  WITH(NOLOCK) ON  U.Id=B.BookedUsrId                              
    WHERE B.Id=@Id;                              
   END                         
  ELSE                         
   BEGIN                              
    SELECT ISNULL(ClientLogo,'') AS ClientLogo,C.ClientName,                              
    B.BookingCode,U.FirstName +' ('+ISNULL(U.UserCode,'')+''+')' AS FirstName,U.Email,ISNULL(U.PhoneNumber,''),B.ClientBookerName,                              
    /*--REPLACE(CONVERT(VARCHAR(11), B.CreatedDate, 106), ' ', '-'),*/                    
 REPLACE(CONVERT(VARCHAR(11), B.BookedDt, 106), ' ', '-'),                              
    B.SpecialRequirements,B.ClientBookerEmail,B.ExtraCCEmail,B.ClientId,B.RowId,B.Client_RequestNo,ISNULL(C.SupportCPhoneNo,'') AS Deskno,
	ISNULL(B.PropertyRefNo,'') AS PropertyRefNo,
	ISNULL(C.SupportEmail,'stay@hummingbirdindia.com') AS SupportEmail                             
    FROM WRBHBBooking B                       
    LEFT OUTER JOIN WRBHBClientManagement C WITH(NOLOCK) ON  C.Id=B.ClientId                              
    LEFT OUTER JOIN WRBHBUser U  WITH(NOLOCK) ON  U.Id=B.BookedUsrId                              
    WHERE B.Id=@Id;                              
   END                              
                      
  /* -- Get CPP & Property Mail */                    
                                
  IF EXISTS(SELECT NULL FROM WRBHBBookingProperty BP                        
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON                              
  BP.BookingId=BG.BookingId AND BP.Id=BG.BookingPropertyTableId AND                              
  BP.PropertyId=BG.BookingPropertyId                              
  WHERE BG.BookingId=@Id AND BP.PropertyType IN ('CPP'))                              
   BEGIN                                
    /* -- CPP Mail */                    
    SET @MailStr=(SELECT TOP 1 ISNULL(D.Email,'')                               
    FROM WRBHBContractClientPref_Header H                              
    LEFT OUTER JOIN WRBHBContractClientPref_Details D                              
    WITH(NOLOCK)ON D.HeaderId=H.Id                              
    WHERE H.IsActive=1 AND H.IsDeleted=0 AND D.IsActive=1 AND                              
    D.IsDeleted=0 AND ISNULL(D.Email,'') != '' AND                           
    D.RoomType IN (SELECT RoomType                              
    FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId=@Id) AND                      
    H.ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id=@Id) AND                              
    D.PropertyId IN (SELECT BookingPropertyId                               
    FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId=@Id));                              
   END                              
  ELSE                              
   BEGIN                              
    /* -- Property Email */                    
    SET @MailStr=(SELECT ISNULL(Email,'') FROM WRBHBProperty                               
    WHERE Id=@BookingPropertyId AND ISNULL(Email,'') != '' AND                              
    IsActive=1 AND IsDeleted=0);                              
   END                              
                      
  SET @Uniglobe = '';                              
  /* -- dataset table 3 */                    
  IF EXISTS(SELECT NULL FROM WRBHBClientSMTP WHERE IsActive = 1 AND                     
  IsDeleted = 0 AND ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id)                               
  AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id AND ClientBookerId <> '0'))                              
   BEGIN                    
    SET @Uniglobe = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id);                      
    /*SELECT BP.PropertyName, '' as UserName,                              
    ISNULL(BP.Email,'') Email,ISNULL(BP.Phone,'') PhoneNumber,                              
    ISNULL(@MailStr,'') --ISNULL(BP.Email,'') AS Email                              
    FROM dbo.WRBHBProperty BP WHERE BP.Id=@BookingPropertyId;*/                                
    SELECT '' AS PropertyName,FirstName AS UserName,'' as Email,                              
    MobileNo AS PhoneNumber,ISNULL(@MailStr,''),                              
    FirstName +' - '+MobileNo+' - '+Email                              
    FROM WRBHBClientManagementAddNewClient                          
    WHERE Id = (SELECT ClientBookerId FROM WRBHBBooking WHERE Id = @Id);                              
   END                              
  ELSE                              
   BEGIN                               
    SELECT '' AS PropertyName, '' AS UserName, '' AS Email, '' AS PhoneNumber, ISNULL(@MailStr,''), ' - stay@hummingbirdindia.com';                    
   END                    
                              
  SELECT @PName=(P.PropertyName+', '+L.Locality+', '+C.CityName),                              
  @SecurityPolicy=P.BookingPolicy,@CancelationPolicy=P.CancelPolicy                              
  FROM WRBHBProperty P                              
  LEFT OUTER JOIN WRBHBCity C WITH(NOLOCK)ON C.Id=P.CityId                              
  LEFT OUTER JOIN WRBHBLocality L WITH(NOLOCK)ON L.Id=P.LocalityId                               
  WHERE P.Id = @BookingPropertyId;                              
                      
  CREATE TABLE #GST(MobileNo NVARCHAR(100));                              
  INSERT INTO #GST(MobileNo)                           
  SELECT STUFF((SELECT ', '+ISNULL(BA.MobileNo,'')                              
  FROM WRBHBBookingGuestDetails BA                              
  WHERE BA.BookingId=G.BookingId AND BA.GuestId=BA.GuestId AND                              
  BA.MobileNo != ''                              
  FOR XML PATH('')),1,1,'') AS MobileNo                              
  FROM WRBHBBookingPropertyAssingedGuest G                              
  WHERE G.BookingId=@Id GROUP BY G.BookingId;                              
                      
  SET @MobileNo = (SELECT TOP 1 ISNULL(MobileNo,'') FROM #GST);                          
                      
  DECLARE @TACPer DECIMAL(27,2) = 0;                              
  DECLARE @AgreedTariff NVARCHAR(1000) = '';                              
  IF EXISTS (SELECT NULL FROM WRBHBBookingProperty P                              
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest G WITH(NOLOCK)ON                              
  G.BookingId=P.BookingId AND G.BookingPropertyTableId=P.Id                        
  WHERE G.BookingId=@Id AND P.GetType='Property' AND                              
  P.PropertyType='ExP')                              
   BEGIN                        
    SELECT @TACPer = TACPer FROM WRBHBBookingProperty                         
    WHERE Id IN (SELECT TOP 1 BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest                   
    WHERE BookingId = @Id);                                
                        
    DECLARE @SingleCnt INT = 0;                              
    SELECT @SingleCnt = COUNT(*) FROM WRBHBBookingPropertyAssingedGuest                              
    WHERE IsActive=1 AND IsDeleted=0 AND BookingId=@Id AND                                  Occupancy = 'Single';                              
                        
    DECLARE @DoubleCnt INT = 0;                              
    SELECT @DoubleCnt = COUNT(*) FROM WRBHBBookingPropertyAssingedGuest                              
    WHERE IsActive=1 AND IsDeleted=0 AND BookingId=@Id AND                              
    Occupancy = 'Double';                              
                        
    DECLARE @TripleCnt INT = 0;                              
    SELECT @TripleCnt = COUNT(*) FROM WRBHBBookingPropertyAssingedGuest                              
    WHERE IsActive=1 AND IsDeleted=0 AND BookingId=@Id AND                              
    Occupancy = 'Triple';                              
                        
    DECLARE @SingleTariff DECIMAL(27,2) = 0;                              
    DECLARE @DoubleTariff DECIMAL(27,2) = 0;               
    DECLARE @TripleTariff DECIMAL(27,2) = 0;                              
    DECLARE @SingleMarkup DECIMAL(27,2) = 0;                              
    DECLARE @DoubleMarkup DECIMAL(27,2) = 0;                              
    DECLARE @TripleMarkup DECIMAL(27,2) = 0;                      
    DECLARE @TACFlag BIT = 0;                              
                              
    /* -- NEW */                    
    SELECT TOP 1 @SingleTariff = P.SingleTariff,@DoubleTariff = P.DoubleTariff,@TripleTariff = P.TripleTariff,                              
    @TACFlag = ISNULL(TAC,0),                              
    @SingleMarkup = (P.SingleandMarkup1 - P.SingleTariff),                              
    @DoubleMarkup = (P.DoubleandMarkup1 - P.DoubleTariff),                              
    @TripleMarkup = (P.TripleandMarkup1 - P.TripleTariff)                              
    FROM WRBHBBookingProperty P                              
    WHERE P.Id IN (SELECT TOP 1 BookingPropertyTableId                               
    FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId = @Id);                              
    /* --CHANGES - MARKUP PERCENTAGE ISSUE PROCESS END */                    
                        
    IF @TACFlag = 0                          
     BEGIN                              
      /* -- CHANGES Markup Amount not shown in BTC START (29 SEP 2015) */                    
      IF @PayMode = 'Direct'                              
       BEGIN                              
        IF @SingleCnt != 0                               
         BEGIN                              
          SET @AgreedTariff = 'Single - Tariff : ' + CAST(@SingleTariff AS VARCHAR) + ', | TAC : ' + CAST(@SingleMarkup AS VARCHAR) + '.<br>';                              
         END        
        IF @DoubleCnt != 0                              
         BEGIN            
          SET @AgreedTariff = @AgreedTariff + 'Double - Tariff : ' + CAST(@DoubleTariff AS VARCHAR) + ', | TAC : ' + CAST(@DoubleMarkup AS VARCHAR) + '.<br>';                              
         END                      
        IF @TripleCnt != 0                              
         BEGIN                              
          SET @AgreedTariff = @AgreedTariff + 'Triple - Tariff : ' + CAST(@TripleTariff AS VARCHAR) + ', | TAC : ' + CAST(@TripleMarkup AS VARCHAR) + '.<br>';                              
         END                              
        SET @AgreedTariff = @AgreedTariff + ' + Taxes as applicable.'                                
       END                               
      IF @PayMode = 'Bill to Company (BTC)'                              
       BEGIN                              
        IF @SingleCnt != 0                               
         BEGIN                              
          SET @AgreedTariff = 'Single - Tariff : ' + CAST(@SingleTariff AS VARCHAR) + '.<br>';                              
         END                              
        IF @DoubleCnt != 0                              
         BEGIN                              
          SET @AgreedTariff = @AgreedTariff + 'Double - Tariff : ' + CAST(@DoubleTariff AS VARCHAR) + '.<br>';                              
         END                              
        IF @TripleCnt != 0                              
         BEGIN                              
          SET @AgreedTariff = @AgreedTariff + 'Triple - Tariff : ' + CAST(@TripleTariff AS VARCHAR) + '.<br>';                              
         END                              
       END                          
      /* -- CHANGES Markup Amount not shown in BTC END (29 SEP 2015) */                    
     END                                
    ELSE                              
     BEGIN                              
      IF @SingleCnt != 0                              
       BEGIN                              
        SET @AgreedTariff =                               
        'Tariff - Single : ' + CAST(@SingleTariff AS VARCHAR)+', ';                              
       END                              
     IF @DoubleCnt != 0                              
       BEGIN                              
        IF @AgreedTariff != ''                              
         BEGIN                              
          SET @AgreedTariff =                               
          @AgreedTariff + 'Double : ' + CAST(@DoubleTariff AS VARCHAR)+ ', ';                              
         END                              
        ELSE                              
        BEGIN               
 SET @AgreedTariff =                               
          'Double : ' + CAST(@DoubleTariff AS VARCHAR) + ', ';                              
         END                              
       END                              
      IF @TripleCnt != 0                              
       BEGIN                              
        IF @AgreedTariff != ''                              
         BEGIN                              
          SET @AgreedTariff =                               
          @AgreedTariff + '<br> Triple : ' + CAST(@TripleTariff AS VARCHAR) + ', ';                              
         END                              
        ELSE                              
         BEGIN                              
          SET @AgreedTariff =                               
          '<br> Triple : ' + CAST(@TripleTariff AS VARCHAR) + ', ';                              
     END                              
   END                              
      SET @AgreedTariff = @AgreedTariff + '<br> TAC : ' + CAST(@TACPer AS VARCHAR)+' % + Taxes as applicable.';                              
     END                              
   END                              
  ELSE                              
   BEGIN              
    DECLARE @Taxes123 NVARCHAR(100) = 'Taxes as applicable';                              
   END                              
                      
  DECLARE @TypeOfProperty NVARCHAR(100) = '';                              
  DECLARE @TypeOfRoom NVARCHAR(100) = '';                   
  SELECT @TypeOfProperty = PropertyType,@TypeOfRoom = RoomType                              
  FROM WRBHBBookingProperty WHERE Id IN                               
  (SELECT TOP 1 BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest                               
  WHERE BookingId = @Id);                              
  DECLARE @PtyRefNo NVARCHAR(100) = '';                              
  DECLARE @PropertyRefNo NVARCHAR(100) = '',@ConfirmedStatus NVARCHAR(MAX),@Confirmedby NVARCHAR(MAX);       
                               
  SELECT @PtyRefNo = ISNULL(PropertyRefNo,'') FROM WRBHBBooking WHERE Id=@Id;        
  SELECT @ConfirmedStatus = ISNULL(ConfirmedStatus,''),@Confirmedby=ISNULL(Confirmedby,'') FROM WRBHBBookingProperty WHERE Id IN                               
  (SELECT TOP 1 BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest                               
  WHERE BookingId = @Id);                              
        
  IF @PtyRefNo = ''                          
   BEGIN                              
    SET @PropertyRefNo = '';          
   END                
 ELSE IF @TypeOfProperty = 'BOK'                
   BEGIN                              
    SET @PropertyRefNo = 'Booking.com reference number - '+@PtyRefNo;                      
   END                              
  ELSE                            
 BEGIN                              
    SET @PropertyRefNo = 'reference number - '+@PtyRefNo;                      
   END                              
                      
  DECLARE @BTCTaxesContent NVARCHAR(1000) = '';                                
  IF @TypeofPtyy NOT IN ('MGH','InP','DdP')                              
   BEGIN                              
    /*SET @BTCTaxesContent = 'Inclusive of Property Taxes (LT & ST) & 7.42% extra ST.<br><p style="font-size:14px;padding-left:5px;font-weight:bold;">                      
    IMPORTANT – Kindly arrange to fill the Proof of Stay (as per your company’s requirement) which has been                             
    shared with the property. Kindly insist to fill the FORM.<p> [Base Tariff : '+@BaseTariff+' + Taxes]'; */                    
    SET @BTCTaxesContent = 'Inclusive of Property Taxes (LT & ST), Service charge excluded & 8.4%, 0.5% extra ST,SBC,KKC on BTC.<br>                           
    <p style="font-size:14px;padding-left:5px;font-weight:bold;">IMPORTANT – Kindly arrange to fill the Proof of Stay (as per your company’s                            
    requirement) which has been shared with the property. Kindly insist to fill the FORM.<p> [Base Tariff : '+@BaseTariff+' + Taxes]';        
    SET @BTCTaxesContent = '';                    
   END                              
  IF @TypeofPtyy IN ('MGH','InP','DdP')                              
   BEGIN                     
    SET @BTCTaxesContent = 'Taxes as applicable. [Base Tariff : '+@BaseTariff+' + Taxes]';                              
   END                              
  IF @TypeOfProperty IN ('CTP', 'OYO', 'TBO', 'BOK')                            
   BEGIN                            
    SET @SecurityPolicy = '<ul><li>A picture of the guest will be taken through webcam for records.</li><li> The guests mobile number and official '+                    
 'e-mail address needs to be provided.</li><li> Government Photo ID proof such as driving license, passport, voter ID card etc. needs to be '+                    
 'produced.</li><li> A company business card or company ID card needs to be produced.</li></ul>';                    
                    
 SELECT TOP 1 @CancelationPolicy = ISNULL(Description1,'') FROM dbo.WRBHBBookingProperty WHERE Id = @BookingPropertyTableId;                    
                    
 IF @CancelationPolicy = ''                    
  BEGIN                    
   SET @CancelationPolicy = '<ul><li> Email to <a href="mailto:stay@hummingbirdindia.com" target="_blank">stay@hummingbirdindia.com</a> and mention the '+                    
   'booking ID no.</li><li>Cancellation less than 48 hrs &nbsp;&ndash; NIL. More than 48 hrs. &ndash; 100% refund.</li><li>1 day tariff will be charged '+                    
   'for no-show without intimation.</li></ul>';                    
  END                      
   END                            
                            
  /* -- dataset table 4 */                    
  SELECT CAST(EmailtoGuest AS INT),                            
  /* --'D:/Backend/flex_bin/Company_Images/Proof_of_Stay.pdf', */                    
  'http://endpoint887127.azureedge.net/img/Proof_of_Stay.pdf',            
  'Proof_of_Stay.pdf', @PName, @MobileNo, @SecurityPolicy, @CancelationPolicy, @Taxes, @TypeOfProperty, @PropertyRefNo, @CLogo, @CLogoAlt, @TypeOfRoom, @BTCTaxesContent,                             
  @Stay, @Uniglobe, 'Powered by HummingBird', @NeedhelpbookingText,@ConfirmedStatus,@Confirmedby                            
  FROM WRBHBBooking                            
  WHERE Id = @Id;                            
                            
  /* -- dataset table 5 */                    
  SELECT DISTINCT EmailId FROM WRBHBBookingGuestDetails WHERE BookingId=@Id;                              
  /* -- Dataset Table 6 */                    
 IF EXISTS (SELECT NULL FROM WRBHBClientwisePricingModel                               
  WHERE IsActive=1 AND IsDeleted=0 AND                               
  ClientId=(SELECT ClientId FROM WRBHBBooking WHERE Id=@Id))                              
   BEGIN                              
    SELECT ClientLogo,ClientName,@CLogo,@CLogoAlt,@cltlogoMGH,@cltaltMGH                               
    FROM WRBHBClientManagement                               
    WHERE IsActive=1 AND IsDeleted=0 AND                              
    Id=(SELECT ClientId FROM WRBHBBooking WHERE Id=@Id);                     
   END                              
  ELSE                              
   BEGIN                              
    SELECT Logo,@CLogoAlt,@CLogo,@CLogoAlt,@cltlogoMGH,@cltaltMGH                               
    FROM WRBHBCompanyMaster                               
    WHERE IsActive=1 AND IsDeleted=0;                        
   END                              
  /* -- dataset table 7 */                    
  SELECT Email FROM dbo.WRBHBClientManagementAddNewClient                               
  WHERE IsActive=1 AND IsDeleted=0 AND ContactType='Extra CC' AND                              
  CltmgntId=(SELECT ClientId FROM WRBHBBooking B WHERE B.Id=@Id);                              
  /*-- Dataset Table 8 */                    
              
  DECLARE @RPId NVARCHAR(100) =                              
  (SELECT TOP 1 CAST(ISNULL(BookingPropertyId,'') AS VARCHAR)                               
  FROM WRBHBBookingPropertyAssingedGuest                            
  WHERE BookingId = @Id GROUP BY BookingPropertyId);                              
                          
  SELECT B.ClientBookerEmail,BP.PropertyType,B.ExtraCCEmail,                              
  'http://sstage.in/qr/'+@RPId+'.png' FROM WRBHBBooking B                              
  LEFT OUTER JOIN WRBHBBookingProperty BP                               
  WITH(NOLOCK)ON BP.BookingId=B.Id                              
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG                              
  WITH(NOLOCK)ON BG.BookingPropertyTableId=BP.Id                               
  WHERE BG.BookingId=@Id                               
  GROUP BY B.ClientBookerEmail,BP.PropertyType,B.ExtraCCEmail;                              
  /* -- Dataset Table 9 Email Address Begin */                    
  CREATE TABLE #Mail(Email VARCHAR(MAX));                              
  /* -- Guest Email */                    
  INSERT INTO #Mail(Email)                              
  SELECT ISNULL(G.EmailId,'') FROM WRBHBBooking B                              
  LEFT OUTER JOIN WRBHBBookingGuestDetails G WITH(NOLOCK)ON                       
  G.BookingId=B.Id  WHERE B.EmailtoGuest=1 AND B.Id=@Id AND ISNULL(G.EmailId,'') != '';                              
  /* -- Booker Email */                    
  INSERT INTO #Mail(Email)                              
  SELECT ISNULL(ClientBookerEmail,'') FROM WRBHBBooking                               
  WHERE Id=@Id AND ISNULL(ClientBookerEmail,'') != '';                              
  /* -- Extra CC Email */                    
  INSERT INTO #Mail(Email)                              
  SELECT ISNULL(Email,'') FROM dbo.WRBHBClientManagementAddNewClient                   
  WHERE IsActive=1 AND IsDeleted=0 AND ContactType='Extra CC' AND                              
  CltmgntId=(SELECT ClientId FROM WRBHBBooking B WHERE B.Id=@Id);                     
                      
  IF EXISTS(SELECT NULL FROM WRBHBBookingProperty BP                              
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON                              
  BP.BookingId=BG.BookingId AND BP.Id=BG.BookingPropertyTableId AND                              
  BP.PropertyId=BG.BookingPropertyId                              
  WHERE BG.BookingId=@Id AND BP.PropertyType IN ('CPP'))        
   BEGIN                              
    INSERT INTO #Mail(Email)                              
    SELECT ISNULL(D.Email,'') FROM WRBHBContractClientPref_Header H                              
    LEFT OUTER JOIN WRBHBContractClientPref_Details D                              
    WITH(NOLOCK)ON D.HeaderId=H.Id                              
    WHERE H.IsActive=1 AND H.IsDeleted=0 AND D.IsActive=1 AND                              
    D.IsDeleted=0 AND ISNULL(D.Email,'') != '' AND               
    H.ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id=@Id) AND                              
    D.PropertyId IN (SELECT BookingPropertyId                               
    FROM WRBHBBookingPropertyAssingedGuest WHERE BookingId=@Id)                              
    GROUP BY D.Email;                               
   END                              
  ELSE                              
 BEGIN                              
    INSERT INTO #Mail(Email)                              
    SELECT ISNULL(Email,'') FROM WRBHBProperty                               
    WHERE Id=@BookingPropertyId AND ISNULL(Email,'') != '';                              
   END                              
  ;WITH tmp(DataItem,Email) AS                               
  (                              
    SELECT LEFT(Email, CHARINDEX(',',Email+',')-1),                              
    STUFF(Email, 1, CHARINDEX(',',Email+','), '') FROM #Mail                              
    UNION ALL                              
SELECT LEFT(Email, CHARINDEX(',',Email+',')-1),                              
    STUFF(Email, 1, CHARINDEX(',',Email+','), '') FROM tmp                              
    WHERE Email > ''                              
   )                              
   SELECT dbo.TRIM(DataItem) AS Email FROM tmp WHERE DataItem != '' GROUP BY DataItem;                              
  /* -- Dataset Table 9 Email Address End */             
                     
  /* -- Dataset Table 10 Begin */                    
   IF EXISTS(SELECT NULL FROM dbo.WRBHBClientSMTP WHERE IsActive = 1 AND IsDeleted = 0 AND                     
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 0) -- CLIENT                    
 BEGIN                    
 SELECT TOP 1 CASE WHEN ISNULL(EmailId,'') = '' THEN 'stay@hummingbirdindia.com' ELSE EmailId END,                    
 CASE WHEN ISNULL(PropertyEmail,'') = '' THEN 'stay@hummingbirdindia.com' ELSE PropertyEmail END                     
 FROM dbo.WRBHBClientSMTP                     
 WHERE IsActive = 1 AND IsDeleted = 0 AND ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 0;                    
 END                    
 ELSE IF EXISTS(SELECT NULL FROM dbo.WRBHBClientSMTP WHERE IsActive = 1 AND IsDeleted = 0 AND                         
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 1) -- BOOKER                    
 BEGIN                    
 SELECT CASE WHEN ISNULL(EmailId,'') = '' THEN 'stay@hummingbirdindia.com' ELSE EmailId END,                    
 CASE WHEN ISNULL(PropertyEmail,'') = '' THEN 'stay@hummingbirdindia.com' ELSE PropertyEmail END                  
 FROM dbo.WRBHBClientSMTP                     
 WHERE IsActive = 1 AND IsDeleted = 0 AND                     
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 1;                    
 END                    
 ELSE                 BEGIN                    
 SELECT top 1 'stay@hummingbirdindia.com' AS EmailId,'stay@hummingbirdindia.com' AS PropertyEmail FROM WRBHBClientSMTP;                    
 END                               
  /* -- Dataset Table 10 End */                    
  DECLARE @BelowTACcontent NVARCHAR(MAX) = 'Kindly arrange to pay the above TAC amount to HummingBird by CHEQUE or through Bank '+                         
  'Transfer (NEFT).<br><b>CHEQUE</b> : Kindly issue the cheque in Favour of "Humming Bird Travel & Stay Pvt Ltd".<br><b>Bank Transfer (NEFT)</b> : <br>Payee '+                         
  'Name : Humming Bird Travel & Stay Pvt. Ltd.<br>Bank Name : HDFC Bank<br>Account No. : 17552560000226<br>IFSC : HDFC0001755';                              
  /* -- Dataset Table 11 bEGIN */                    
  CREATE TABLE #PropertyMailBTCChecking(Name NVARCHAR(100),                              
  ChkInDt NVARCHAR(100),ChkOutDt NVARCHAR(100),Tariff NVARCHAR(100),                              
  Occupancy NVARCHAR(100),TariffPaymentMode NVARCHAR(100),                              
  ServicePaymentMode NVARCHAR(100),RoomNo NVARCHAR(100));                              
  INSERT INTO #PropertyMailBTCChecking(Name,ChkInDt,ChkOutDt,Tariff,                              
  Occupancy,TariffPaymentMode,ServicePaymentMode,RoomNo)                              
  SELECT B.NameWthTitle,B.ChkInDt,B.ChkOutDt,B.Tariff,B.Occupancy,B.TariffPaymentMode,                              
  B.ServicePaymentMode,B.RoomNo FROM #QAZ B;                              
  IF EXISTS(SELECT NULL FROM WRBHBBookingProperty BP                              
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON                              
  BP.BookingId=BG.BookingId AND BP.Id=BG.BookingPropertyTableId AND                              
  BP.PropertyId=BG.BookingPropertyId                              
  WHERE BG.BookingId=@Id AND BG.TariffPaymentMode='Bill to Company (BTC)' AND                              
  BP.PropertyType IN ('ExP'))                              
   BEGIN                              
    DECLARE @Single DECIMAL(27,2),@Double DECIMAL(27,2),@Triple DECIMAL(27,2);                              
    SELECT @Single=SingleTariff,@Double=DoubleTariff,@Triple=TripleTariff                            
FROM WRBHBBookingProperty BP                                
    LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON                              
    BP.BookingId=BG.BookingId AND BP.Id=BG.BookingPropertyTableId AND                              
    BP.PropertyId=BG.BookingPropertyId                              
    WHERE BG.BookingId=@Id; 
	
	    
    SELECT Name,ChkInDt,ChkOutDt,                              
		CASE WHEN
		CASE WHEN Occupancy = 'Single' THEN @Single                              
         WHEN Occupancy = 'Double' THEN @Double                              
         WHEN Occupancy = 'Triple' THEN @Triple                              
         ELSE Tariff END  >= 500 THEN
         CAST(CASE WHEN Occupancy = 'Single' THEN @Single                              
         WHEN Occupancy = 'Double' THEN @Double                              
         WHEN Occupancy = 'Triple' THEN @Triple                              
         ELSE Tariff END AS VARCHAR)+'/-' + '<br>  + Taxes' ELSE
        CAST(CASE WHEN Occupancy = 'Single' THEN @Single                              
         WHEN Occupancy = 'Double' THEN @Double                              
         WHEN Occupancy = 'Triple' THEN @Triple                              
         ELSE Tariff END AS VARCHAR)+'/-' END 
		 ,Occupancy,TariffPaymentMode,                              
    ServicePaymentMode,'BTC',@AgreedTariff,@BelowTACcontent,RoomNo                      
    FROM #PropertyMailBTCChecking;                              
   END                        
  ELSE IF EXISTS(SELECT NULL FROM WRBHBBookingProperty BP                      
  LEFT OUTER JOIN WRBHBBookingPropertyAssingedGuest BG WITH(NOLOCK)ON                              
  BP.BookingId=BG.BookingId AND BP.Id=BG.BookingPropertyTableId AND                              
  BP.PropertyId=BG.BookingPropertyId                              
  WHERE BG.BookingId=@Id AND BP.PropertyType IN ('CPP'))                             
   BEGIN                        
    DECLARE @CPPTACPer DECIMAL(27,2),@CPPSingle DECIMAL(27,2),@CPPDouble DECIMAL(27,2);                        
 DECLARE @CPPOccupancy NVARCHAR(100),@CPPTriple DECIMAL(27,2);                        
    SELECT TOP 1 @CPPTACPer = ISNULL(TACPer,0),@CPPSingle = ISNULL(SingleTariff,0),                        
 @CPPDouble = ISNULL(DoubleTariff,0)                        
 FROM WRBHBBookingProperty WHERE Id IN                        
 (SELECT TOP 1 BookingPropertyTableId FROM WRBHBBookingPropertyAssingedGuest                         
 WHERE BookingId = @Id AND IsActive = 1);                        
 SELECT TOP 1 @CPPOccupancy=Occupancy FROM WRBHBBookingPropertyAssingedGuest                        
 WHERE BookingId = @Id AND IsActive = 1;                         
 IF @CPPTACPer > 0                        
  BEGIN                        
   IF @CPPOccupancy = 'Single'                        
    BEGIN                        
     SET @AgreedTariff = 'TAC Amount : '+CAST(CAST(ROUND((@CPPSingle * @CPPTACPer)/100,0) AS DECIMAL(27,2)) AS VARCHAR);                        
    END                        
ELSE IF @CPPOccupancy = 'Double'                        
    BEGIN                        
 SET @AgreedTariff = 'TAC Amount : '+CAST(CAST(ROUND((@CPPDouble * @CPPTACPer)/100,0) AS DECIMAL(27,2)) AS VARCHAR);                        
    END                       
   ELSE                        
    BEGIN                        
     SET @AgreedTariff = 'TAC Amount : '+CAST(CAST(ROUND((@CPPTriple * @CPPTACPer)/100,0) AS DECIMAL(27,2)) AS VARCHAR);                        
    END                         
  END                        
 ELSE                        
  BEGIN                        
   SET @AgreedTariff = '';                        
  END                        
    SELECT Name,ChkInDt,ChkOutDt,                              
    CASE WHEN @TXADED = 'T' THEN 
	CASE WHEN CAST(Tariff AS DECIMAL(27,2)) >= 500 THEN Tariff +'/-' + '<br>  + Taxes' ELSE
    Tariff +'/-' END 
    WHEN @TXADED = 'N' AND Tariff != '0.00' THEN Tariff+'/- <br>(Nett Tariff) '                              
    WHEN @TXADED = 'N' AND Tariff = '0.00' THEN Tariff                                
         ELSE CASE WHEN CAST(Tariff AS DECIMAL(27,2)) >= 500 THEN Tariff +'/-' + '<br>  + Taxes' ELSE
    CAST(Tariff AS VARCHAR)+'/-' END  END,Occupancy,TariffPaymentMode,                              
    ServicePaymentMode,'NOTBTC',@AgreedTariff,@BelowTACcontent,RoomNo                              
    FROM #PropertyMailBTCChecking;                              
   END                              
  ELSE                              
   BEGIN                              
    SELECT Name,ChkInDt,ChkOutDt,                              
    CASE WHEN @TXADED = 'T' THEN CASE WHEN CAST(Tariff AS DECIMAL(27,2)) >= 500 THEN Tariff +'/-' + '<br>  + Taxes' ELSE
     Tariff +'/-' END                                 
   WHEN @TXADED = 'N' AND Tariff != '0.00' THEN Tariff+'/- <br>(Nett Tariff) '                              
         WHEN @TXADED = 'N' AND Tariff = '0.00' THEN Tariff                               
         ELSE CASE WHEN Tariff >= 500 THEN  Tariff +'/-' + '<br>  + Taxes' ELSE
    Tariff +'/-' END  END,Occupancy,TariffPaymentMode,                              
    ServicePaymentMode,'NOTBTC',@AgreedTariff,@BelowTACcontent,RoomNo                               
    FROM #PropertyMailBTCChecking;                              
   END                              
  /*-- Dataset Table 11 eND */                    
if exists(select b.Id from WRBHBBooking b                     
left outer join WRBHBBookingProperty p with(nolock)on b.Id = p.BookingId                     
where b.IsActive = 1 and b.IsDeleted = 0 and p.IsActive = 1 and p.IsDeleted = 0 and b.Id = @Id and                     
datediff(MINUTE,b.BookedDt,getdate()) <= 1 and p.PropertyType in ('exp','cpp'))                     
begin                     
declare @min int,@clientid bigint,@propertyId bigint,@propertyType varchar(100),@roomid bigint,@roomtype varchar(100);                     
select top 1  @min = datediff(MINUTE,b.BookedDt,getdate()),@clientid = b.ClientId,@PropertyId = p.PropertyId,@PropertyType = p.PropertyType,                     
@roomid = p.RoomTypeCode,@RoomType = g.RoomType from WRBHBBooking b                     
left outer join WRBHBBookingProperty p with(nolock)on b.Id = p.BookingId                     
left outer join WRBHBBookingPropertyAssingedGuest g with(nolock)on p.BookingId = g.BookingId and p.Id = g.BookingPropertyTableId                     
where b.IsActive = 1 and b.IsDeleted = 0 and p.IsActive = 1 and p.IsDeleted = 0 and g.IsActive = 1 and g.IsDeleted = 0 and b.Id = @Id;                     
--                     
declare @racksingle decimal(27,2),@rackdouble decimal(27,2),@racktriple decimal(27,2);                     
--                     
if @PropertyType = 'exp'              
begin                     
select top 1 @racksingle = isnull(Single,0),@rackdouble = isnull(RDouble,0),@racktriple = isnull(Triple,0) from WRBHBPropertyAgreementsRoomCharges                     
where Id = @roomid and RoomType = @roomtype;                     
--                     
update WRBHBBookingPropertyAssingedGuest set RackSingle = @racksingle, RackDouble = @rackdouble, RackTriple = @racktriple                      
where BookingId = @Id and IsActive = 1 and IsDeleted = 0;                     
end                     
if @PropertyType = 'cpp'                     
begin                     
select top 1 @racksingle = isnull(RTariffSingle,0), @rackdouble = isnull(RTariffDouble,0), @racktriple = isnull(RTariffTriple,0) from WRBHBContractClientPref_Header h                     
left outer join WRBHBContractClientPref_Details d with(nolock)on h.Id = d.HeaderId                     
where h.IsActive = 1 and h.IsDeleted = 0 and d.IsActive = 1 and d.IsDeleted = 0 and h.ClientId = @clientid and d.PropertyId = @propertyId and d.RoomType = @roomtype                    
order by d.ModifiedDate desc;                     
--                     
update WRBHBBookingPropertyAssingedGuest set RackSingle = @racksingle, RackDouble = @rackdouble, RackTriple = @racktriple                      
where BookingId = @Id and IsActive = 1 and IsDeleted = 0;                     
end                     
end                     
                               
END                  
                    
IF @Action = 'BedBookingConfirmed'                              
 BEGIN                              
  /*  -- Dataset Table 0  */            
  SELECT BA.FirstName,                              
  REPLACE(CONVERT(VARCHAR(11),BA.ChkInDt, 106), ' ', '-') +' / '+                               
  LEFT(ExpectedChkInTime, 5)+' '+BA.AMPM,                              
  REPLACE(CONVERT(VARCHAR(11), BA.ChkOutDt, 106), ' ', '-'),                              
  CAST(BA.Tariff AS VARCHAR)+'/-',                              
  CASE WHEN BA.TariffPaymentMode = 'Direct' THEN 'Direct<br>(Cash/Card)'                      
       WHEN BA.TariffPaymentMode = 'Bill to Client' THEN 'Bill to '+@ClientName1                        
       ELSE BA.TariffPaymentMode END AS TariffPaymentMode,                        
  CASE WHEN BA.ServicePaymentMode='Direct' THEN 'Direct<br>(Cash/Card)'                              
       WHEN BA.ServicePaymentMode = 'Bill to Client' THEN 'Bill to '+@ClientName1                                
       ELSE BA.ServicePaymentMode END AS ServicePaymentMode,                              
       BA.BedType,'Single',BP.BookingLevel,LEFT(ExpectedChkInTime, 5)+' '+BA.AMPM,convert(varchar, BA.ChkInDt, 107),
	   convert(varchar, BA.ChkOutDt, 107),BA.Title             
  FROM WRBHBBooking BP                              
  LEFT OUTER JOIN dbo.WRBHBBedBookingPropertyAssingedGuest BA                               
  WITH(NOLOCK)ON BP.Id=BA.BookingId AND BA.IsActive=1 AND BA.IsDeleted=0                              
  WHERE BP.Id=@Id AND BP.IsActive=1 AND BP.IsDeleted=0 AND                          
  BA.RoomShiftingFlag = 0;                              
              
  /* -- Get Booking Property Id */            
  SET @BookingPropertyId=(SELECT TOP 1 BookingPropertyId                               
  FROM WRBHBBedBookingPropertyAssingedGuest WHERE IsActive=1 AND IsDeleted=0                               
  AND BookingId=@Id);                          
              
  /* -- Dataset Table 1 */            
  SELECT BP.Propertaddress+', '+L.Locality+', '+                              
  C.CityName+', '+S.StateName+' - '+BP.Postal,                    
  BP.Phone,BP.Directions,BP.BookingPolicy,BP.CancelPolicy,BP.PropertyName,                              
  BP.Category,ISNULL(BP.CheckOutType,'') CheckOutType,                              
  ISNULL(CheckIn,'') CheckIn,ISNULL(CheckInType,'') CheckInType,                              
  ISNULL(CheckOut,'') CheckOut,T.PropertyType,                    
  Phone,REPLACE(BP.PropertyName, ' ', '+')+'/@'+REPLACE(BP.LatitudeLongitude, ' ', '')+',15z' AS Map,ExtraNote                      
  FROM dbo.WRBHBProperty BP                              
  LEFT OUTER JOIN WRBHBLocality L WITH(NOLOCK) ON L.Id=BP.LocalityId                              
  LEFT OUTER JOIN WRBHBCity C WITH(NOLOCK) ON L.CityId=C.Id               
  LEFT OUTER JOIN WRBHBState S WITH(NOLOCK) ON S.Id=C.StateId                              
  LEFT OUTER JOIN dbo.WRBHBPropertyType T WITH(NOLOCK) ON T.Id=BP.PropertyType                                
  WHERE BP.Id=@BookingPropertyId  AND BP.IsActive=1 AND BP.IsDeleted=0;                              
             
  /* -- Dataset Table 2 */            
  IF EXISTS(SELECT NULL FROM WRBHBBooking WHERE Id = @Id AND                      
  ISNULL(HBStay,'') = 'StayCorporateHB')                              
   BEGIN                              
    SELECT ISNULL(ClientLogo,'') AS ClientLogo,C.ClientName,                              
    B.BookingCode,U.FirstName,U.Email,U.Mobile,B.ClientBookerName,            
    REPLACE(CONVERT(VARCHAR(11), B.BookedDt, 106), ' ', '-'),                              
    B.SpecialRequirements,B.ClientBookerEmail,B.ClientId,B.RowId,B.Client_RequestNo,ISNULL(C.SupportCPhoneNo,'') AS Deskno, 
	ISNULL(C.SupportEmail,'stay@hummingbirdindia.com') AS SupportEmail FROM WRBHBBooking B                              
    LEFT OUTER JOIN WRBHBClientManagement C WITH(NOLOCK) ON  C.Id=B.ClientId                              
    LEFT OUTER JOIN WrbhbTravelDesk U  WITH(NOLOCK) ON  U.Id=B.BookedUsrId                              
    WHERE B.Id=@Id;                              
   END                              
  ELSE                              
   BEGIN                           
    SELECT ISNULL(ClientLogo,'') AS ClientLogo,C.ClientName,                              
    B.BookingCode,U.FirstName+' ('+ISNULL(U.UserCode,'')+''+')' AS FirstName,U.Email,U.PhoneNumber,B.ClientBookerName,            
    REPLACE(CONVERT(VARCHAR(11), B.BookedDt, 106), ' ', '-'),                              
    B.SpecialRequirements,B.ClientBookerEmail,B.ClientId,B.RowId,B.Client_RequestNo,ISNULL(C.SupportCPhoneNo,'') AS Deskno, 
	ISNULL(C.SupportEmail,'stay@hummingbirdindia.com') AS SupportEmail FROM WRBHBBooking B                              
    LEFT OUTER JOIN WRBHBClientManagement C WITH(NOLOCK) ON  C.Id=B.ClientId                              
    LEFT OUTER JOIN WRBHBUser U  WITH(NOLOCK) ON  U.Id=B.BookedUsrId                              
    WHERE B.Id=@Id;                              
   END                              
  --                              
  SET @MailStr=(SELECT ISNULL(Email,'') FROM WRBHBProperty                              
  WHERE Id = (select top 1 BookingPropertyId                               
  from WRBHBBedBookingPropertyAssingedGuest where BookingId = @Id));                              
              
  /* -- Dataset Table 3 */            
  SET @Uniglobe = '';                              
  IF EXISTS(SELECT NULL FROM WRBHBClientSMTP WHERE IsActive = 1 AND                               
  IsDeleted = 0 AND ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id)                               
  AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id AND ClientBookerId <> '0'))                                
   BEGIN                              
  SET @Uniglobe = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id);            
    SELECT '' AS PropertyName,FirstName AS UserName,Email,                              
    MobileNo AS PhoneNumber,ISNULL(@MailStr,'')                              
    FROM WRBHBClientManagementAddNewClient                              
    WHERE Id = (SELECT ClientBookerId FROM WRBHBBooking WHERE Id = @Id);                              
   END                        
  ELSE                              
   BEGIN                              
   SELECT '' as PropertyName, '' as UserName, '' as Email,'' as PhoneNumber, ISNULL(@MailStr,'');            
   END            
                                 
  SELECT @PName=(P.PropertyName+', '+L.Locality+', '+C.CityName),                              
  @SecurityPolicy=P.BookingPolicy,@CancelationPolicy=P.CancelPolicy                              
  FROM WRBHBProperty P                              
  LEFT OUTER JOIN WRBHBCity C WITH(NOLOCK)ON C.Id=P.CityId                              
  LEFT OUTER JOIN WRBHBLocality L WITH(NOLOCK)ON L.Id=P.LocalityId                               
  WHERE P.Id = @BookingPropertyId;                              
              
  CREATE TABLE #GST1(MobileNo NVARCHAR(100));                              
  INSERT INTO #GST1(MobileNo)                              
  SELECT STUFF((SELECT ', '+ISNULL(BA.MobileNo,'')                              
  FROM WRBHBBookingGuestDetails BA                 
  WHERE BA.BookingId=G.BookingId AND BA.GuestId=BA.GuestId AND                              
  BA.MobileNo != ''                              
  FOR XML PATH('')),1,1,'') AS MobileNo                              
  FROM WRBHBBedBookingPropertyAssingedGuest G                              
  WHERE G.BookingId=@Id GROUP BY G.BookingId;                              
              
  SET @MobileNo = (SELECT TOP 1 ISNULL(MobileNo,'') FROM #GST1);            
            
  /* -- Dataset Table 4 */            
  SELECT CAST(EmailtoGuest AS INT),@PName,@MobileNo,@SecurityPolicy,@CancelationPolicy,@Stay,@Uniglobe,'Powered by HummingBird',@NeedhelpbookingText,                     
  ISNULL(PropertyRefNo,'') AS PropertyRefNo FROM WRBHBBooking WHERE Id = @Id;                              
 DECLARE @BedBookingPropertyType NVARCHAR(100) = '';                              
  SELECT TOP 1 @BedBookingPropertyType = PropertyType FROM WRBHBBedBookingProperty                               
  WHERE Id IN (SELECT TOP 1 BookingPropertyTableId                               
  FROM WRBHBBedBookingPropertyAssingedGuest WHERE BookingId = @Id);                    
              
  /* -- Dataset Table 5 */            
  SELECT DISTINCT dbo.TRIM(EmailId),@BedBookingPropertyType FROM WRBHBBookingGuestDetails                               
  WHERE BookingId=@Id;                              
              
  /* -- Dataset Table 6 */            
  IF EXISTS (SELECT NULL FROM WRBHBClientwisePricingModel                        
  WHERE IsActive=1 AND IsDeleted=0 AND                               
  ClientId=(SELECT ClientId FROM WRBHBBooking WHERE Id=@Id))                              
   BEGIN                              
    SELECT ClientLogo,ClientName,@CLogo,@CLogoAlt,@cltlogoMGH,@cltaltMGH                               
    FROM WRBHBClientManagement                               
    WHERE IsActive=1 AND IsDeleted=0 AND                              
    Id=(SELECT ClientId FROM WRBHBBooking WHERE Id=@Id);                     
   END                              
  ELSE                              
   BEGIN                              
    SELECT Logo,@CLogoAlt,@CLogo,@CLogoAlt,@cltlogoMGH,@cltaltMGH                              
    FROM WRBHBCompanyMaster                               
    WHERE IsActive=1 AND IsDeleted=0;                              
   END                              
              
  /* -- Dataset table 7 */            
  SELECT dbo.TRIM(Email) FROM dbo.WRBHBClientManagementAddNewClient                               
  WHERE IsActive=1 AND IsDeleted=0 AND ContactType='Extra CC' AND                              
  CltmgntId=(SELECT ClientId FROM WRBHBBooking B WHERE B.Id=@Id);                              
              
  /* -- Dataset Table 8 */            
  DECLARE @BPId NVARCHAR(100) = (SELECT TOP 1 CAST(ISNULL(BookingPropertyId,'') AS VARCHAR) FROM WRBHBBedBookingPropertyAssingedGuest WHERE BookingId = @Id GROUP BY BookingPropertyId);            
            
  SELECT ClientBookerEmail,ExtraCCEmail,'http://sstage.in/qr/'+@BPId+'.png' FROM WRBHBBooking WHERE Id=@Id;            
            
  /* -- Dataset Table 9 Email Address Begin */            
  CREATE TABLE #BedMail(Id INT,Email NVARCHAR(max));                              
              
  /* -- Guest Email */            
  INSERT INTO #BedMail(Id,Email)                              
  SELECT 0,ISNULL(G.EmailId,'') FROM WRBHBBooking B                              
  LEFT OUTER JOIN WRBHBBookingGuestDetails G WITH(NOLOCK)ON                          
  G.BookingId=B.Id                              
 WHERE B.EmailtoGuest=1 AND B.Id=@Id;                              
              
  /* -- Booker Email */            
  INSERT INTO #BedMail(Id,Email)                              
  SELECT 0,ISNULL(ClientBookerEmail,'') FROM WRBHBBooking                               
  WHERE Id=@Id AND ISNULL(ClientBookerEmail,'') != '';            
            
  /* -- Extra CC Email */            
  INSERT INTO #BedMail(Id,Email)                              
  SELECT 0,ISNULL(Email,'') FROM dbo.WRBHBClientManagementAddNewClient                               
  WHERE IsActive=1 AND IsDeleted=0 AND ContactType='Extra CC' AND                              
  CltmgntId=(SELECT ClientId FROM WRBHBBooking B WHERE B.Id=@Id);            
            
  /* -- Property Email */            
  SET @PropertyEmail=(SELECT ISNULL(Email,'') FROM WRBHBProperty                               
  WHERE Id=@BookingPropertyId AND ISNULL(Email,'') != '');                     
  INSERT INTO #BedMail(Id,Email)                              
  SELECT * FROM dbo.Split(@PropertyEmail, ',');            
            
  /* -- Final Select */            
  SELECT dbo.TRIM(Email) FROM #BedMail WHERE Email != '';            
            
  /* -- Dataset Table 9 Email Address End */            
            
  /* -- Dataset Table 10 Begin */            
            
 IF EXISTS(SELECT NULL FROM dbo.WRBHBClientSMTP WHERE IsActive = 1 AND IsDeleted = 0 AND                     
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 0) -- CLIENT                    
 BEGIN                    
 SELECT TOP 1 CASE WHEN ISNULL(EmailId,'') = '' THEN 'stay@hummingbirdindia.com' ELSE EmailId END,                    
 CASE WHEN ISNULL(PropertyEmail,'') = '' THEN 'stay@hummingbirdindia.com' ELSE PropertyEmail END                     
 FROM dbo.WRBHBClientSMTP                     
 WHERE IsActive = 1 AND IsDeleted = 0 AND ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 0;               
 END                    
 ELSE IF EXISTS(SELECT NULL FROM dbo.WRBHBClientSMTP WHERE IsActive = 1 AND IsDeleted = 0 AND                       
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 1) -- BOOKER                            
 BEGIN                    
 SELECT CASE WHEN ISNULL(EmailId,'') = '' THEN 'stay@hummingbirdindia.com' ELSE EmailId END,              
 CASE WHEN ISNULL(PropertyEmail,'') = '' THEN 'stay@hummingbirdindia.com' ELSE PropertyEmail END                              
 FROM dbo.WRBHBClientSMTP                     
 WHERE IsActive = 1 AND IsDeleted = 0 AND                     
 ClientId = (SELECT ClientId FROM WRBHBBooking WHERE Id = @Id) AND EmailId = (SELECT ClientBookerEmail FROM WRBHBBooking WHERE Id = @Id) AND BookerFlag = 1;                    
 END                    
 ELSE                    
 BEGIN                    
 SELECT top 1 'stay@hummingbirdindia.com' AS EmailId,'stay@hummingbirdindia.com' AS PropertyEmail FROM WRBHBClientSMTP;                    
 END                    
  /* -- Dataset Table 10 End */            
 END            

END