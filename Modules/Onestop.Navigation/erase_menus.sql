DECLARE @IdsToDelete TABLE
(
	Id int
)

-- Fetching IDs of items to remove --

INSERT INTO @IdsToDelete(Id)
SELECT CI.Id 
FROM varv_Orchard_Framework_ContentItemRecord CI
INNER JOIN varv_Orchard_Framework_ContentTypeRecord CT
ON CI.ContentType_id = CT.Id
WHERE 
	(CT.Name LIKE '%MenuItem%') OR 
	(CT.Name = 'Menu')

-- Cleaning up Onestop.Navigation tables --

DELETE FROM varv_Onestop_Navigation_ExtendedMenuItemPartRecord
DELETE FROM varv_Onestop_Navigation_VersionInfoPartRecord
DELETE FROM varv_Onestop_Navigation_ImageMenuItemPartRecord

------ Cleaning up the related tables ------

	-- Removing main item records --
	DELETE FROM varv_Orchard_Framework_ContentItemRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing main item related version records --
	DELETE FROM varv_Orchard_Framework_ContentItemVersionRecord
	WHERE ContentItemRecord_id IN (SELECT Id FROM @IdsToDelete)

	-- Removing CommonPart records --
	DELETE FROM varv_Common_CommonPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing CommonPart version records --
	DELETE FROM varv_Common_CommonPartVersionRecord
	WHERE ContentItemRecord_id IN (SELECT Id FROM @IdsToDelete)

	-- Removing IdentityPart records --
	DELETE FROM varv_Common_IdentityPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing TitlePart records --
	DELETE FROM varv_Title_TitlePartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing IdentityPart records --
	DELETE FROM varv_Common_BodyPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing MenuPart records --
	DELETE FROM varv_Navigation_MenuPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing MenuItemPart records --
	DELETE FROM varv_Navigation_MenuItemPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

	-- Removing ContentMenuItemPart records --
	DELETE FROM varv_Navigation_ContentMenuItemPartRecord
	WHERE Id IN (SELECT Id FROM @IdsToDelete)

