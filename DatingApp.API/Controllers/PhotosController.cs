﻿using AutoMapper;using CloudinaryDotNet;using CloudinaryDotNet.Actions;using DatingApp.API.Data;using DatingApp.API.DTOs;using DatingApp.API.Helpers;using DatingApp.API.Models;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Options;using System;using System.Collections.Generic;using System.Linq;using System.Security.Claims;using System.Threading.Tasks;namespace DatingApp.API.Controllers{	[Authorize]	[Route("api/users/{userId}/photos")]	[ApiController]	public class PhotosController: ControllerBase	{		private readonly IDatingRepository repo;		private readonly IMapper mapper;		private readonly IOptions<CloudinarySettings> cloudinaryConfig;		private readonly Cloudinary cloudinary;		public PhotosController(IDatingRepository repo, IMapper mapper, 			IOptions<CloudinarySettings> cloudinaryConfig)		{			this.repo = repo;            			this.mapper = mapper;			this.cloudinaryConfig = cloudinaryConfig;			Account acc = new Account(				cloudinaryConfig.Value.CloudName,				cloudinaryConfig.Value.ApiKey,				cloudinaryConfig.Value.ApiSecret				);			cloudinary = new Cloudinary(acc);		}		[HttpGet("{id}", Name = "GetPhoto")]		public async Task<IActionResult> GetPhoto(int id)
		{
			var photoFromRepo = await repo.GetPhoto(id);
			var photo = mapper.Map<PhotoForReturnDto>(photoFromRepo);
			return Ok(photo);
		}		[HttpPost]		public async Task<IActionResult> AddPhotosForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)		{			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))				return Unauthorized();			var userFromrepo = await repo.GetUser(userId);			var file = photoForCreationDto.File;			var uploadResult = new ImageUploadResult();			if (file.Length > 0)			{				using (var stream = file.OpenReadStream())				{					var uploadParams = new ImageUploadParams()					{						File = new FileDescription(file.Name, stream),						Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")					};                				uploadResult = cloudinary.Upload(uploadParams);				};			}			photoForCreationDto.Url = uploadResult.Uri.ToString();			photoForCreationDto.PublicId = uploadResult.PublicId;			var photo = mapper.Map<Photo>(photoForCreationDto);			if (!userFromrepo.Photos.Any(u => u.IsMain))				photo.IsMain = true;			userFromrepo.Photos.Add(photo);

			if (await repo.SaveAll())
			{
				var photoToReturn = mapper.Map<PhotoForReturnDto>(photo);
				return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
			}			return BadRequest("Could not add the photo");		}		[HttpPost("{id}/setMain")]		public async Task<IActionResult> SetMainPhoto(int userId, int id)
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))				return Unauthorized();			

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
		}		[HttpDelete("{id}")]		public async Task<IActionResult> DeletePhoto(int userId, int id)
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))				return Unauthorized();

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

		}				}}