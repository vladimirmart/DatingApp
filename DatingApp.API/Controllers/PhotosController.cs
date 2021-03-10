﻿using AutoMapper;
		{
			var photoFromRepo = await repo.GetPhoto(id);
			var photo = mapper.Map<PhotoForReturnDto>(photoFromRepo);
			return Ok(photo);
		}

			if (await repo.SaveAll())
			{
				var photoToReturn = mapper.Map<PhotoForReturnDto>(photo);
				return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
			}
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))

			var user = await repo.GetUser(userId);

			if (!user.Photos.Any(p => p.Id == id))
				return Unauthorized();

			var photoFromRepo = await repo.GetPhoto(id);

			if (photoFromRepo.IsMain)			
				return BadRequest("This is already the main photo");

			var currentMainPhoto = await repo.GetMainPhotoForUser(userId);
			currentMainPhoto.IsMain = false;
			photoFromRepo.IsMain = true;		


			if (await repo.SaveAll())
				return NoContent();
			return BadRequest("Could not made the main Photo");
		}
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))

			var user = await repo.GetUser(userId);

			if (!user.Photos.Any(p => p.Id == id))
				return Unauthorized();

			var photoFromRepo = await repo.GetPhoto(id);

			if (photoFromRepo.IsMain)
				return BadRequest("You cannot delete your main photo");

			if (photoFromRepo.PublicId != null)
			{

				var deleteParams = new DeletionParams(photoFromRepo.PublicId);
				var result = cloudinary.Destroy(deleteParams);
				if (result.Result == "ok")
				{
					repo.Delete(photoFromRepo);
				}
			}
			else
			{
				repo.Delete(photoFromRepo);
			}

			if (await repo.SaveAll())
				return Ok();

			return BadRequest("Failed to delete the photo");

		}