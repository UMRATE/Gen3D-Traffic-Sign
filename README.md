# Generalized Traffic Sign Recognition and Localization through Synthetic Data Generation and 3D Semantic Reconstruction

## Table of Contents
- [Overview](#overview)
- [Repository Contents](#repository-contents)
  - [Key Components](#key-components)
  - [Detailed Workflow](#detailed-workflow)
- [Notes](#notes)
- [Contact](#contact)

## Overview
This research introduces a versatile, automated approach to traffic sign management through synthetic data generation and 3D semantic reconstruction. The workflow is outlined as follows:

1. **Synthetic Data Generation**: A virtual reality platform generates a large-scale dataset of annotated synthetic traffic sign images automatically.
2. **Deep Learning-Based Detection**: Synthetic images are utilized to train deep learning models for traffic sign detection, with studies conducted to optimize recognition performance using synthetic data.
3. **3D Semantic Reconstruction**: Photogrammetry is employed to reconstruct 3D models, integrated with detection masks to generate, classify, and localize traffic signs within a 3D semantic point cloud.
4. **Geospatial Alignment**: Camera trajectories are combined with real-world Global Navigation Satellite System (GNSS) signals to enhance the geospatial accuracy of sign localization.
<table>
  <tr>
    <td align="center">
      <img src="overview.png" alt="overview" width="800">
      <br>
      Fig 1. Workflow of the proposed DL and photogrammetry-integrated approach
    </td>
  </tr>
</table>
  
## Repository Contents
This repository contains code for the proposed methodology, including:
1. **Traffic Sign Generation**: Scripts to identify roads in a virtual environment and generate traffic signs according to the defined workflow.
2. **Camera Placement and Mask Generation**: Code to position cameras and generate mask images using raycasting.
3. **Format Conversion Scripts**: Tools for converting data into compatible formats.
4. **3D Semantic Point Cloud Processing**: Scripts for generating, classifying, and localizing traffic signs in a 3D semantic point cloud.

### Key Components
1. **Traffic Sign Synthetic Dataset Generation**: The DatasetGenerator are written in **C#** and designed to run in a unreal engine. [Unity](https://unity.com/cn) was used in this study, specific steps are as follows:

   a. Import the city scene (Fig 2) from the [Unity Asset Store](https://assetstore.unity.com/?srsltid=AfmBOopX2Y6pVpbDR0U101dbc8TpX8v4A-gY8tA5f4f-Qa6QdKLMuj3K)  
   b. Import the provided DatasetGenerator scripts.   
   c. Tag road objects as `Road` in the city scene.  
  
   Run the scripts to:  
     - Identify roads and place traffic signs in a predefined order.  
     - Position cameras and generate mask images of the camera's field of view using raycasting.  
     - Output synthetic images with traffic sign annotations.  

3. **Model Training**:
  The generated synthetic dataset can be used to train the **YOLOv8** model. Refer to the official website: [ultralytics YOLO Vision](https://docs.ultralytics.com/zh).

4. **Traffic Sign Localization**:
  Videos captured by a **DJI drone** are processed for traffic sign recognition.
  The 3D point cloud as shown in Fig 3 reconstructed from [Metashape](https://www.agisoft.com/) undergoes semantic segmentation to obtain precise geospatial coordinates of traffic signs.
    <table>
      <tr>
        <td align="center">
          <img src="city_scene.png" alt="City Scene" width="340">
          <br>
          Fig 2. Pre-constructed virtual city model
        </td>
        <td align="center">
          <img src="3D_point_cloud.png" alt="3D Point Cloud" width="350">
          <br>
          Fig 3. 3D point cloud reconstruction
        </td>
      </tr>
    </table>

### Detailed Workflow
A complete step-by-step workflow for running the synthetic environment is summarized below. Full methodological details will be provided once the accompanying manuscript completes peer review.

1. **Preparing the Virtual Environment**  
   To execute the DatasetGenerator scripts, a Unity project must be set up with a compatible 3D city model：  
   a. Create a new Unity project.  
   b. Import the city scene from the Sketchfab into Unity.  
   c. Import the DatasetGenerator folder from this repository into the Unity Assets/ directory.  
   d. Drag the DatasetGenerator scripts into the scene hierarchy.  
   e. Tag all road surfaces as Road to enable the automatic road-identification module.  
   f. Place the 3D traffic sign prefabs (from Sketchfab or your licensed sources) under a dedicated folder such as Assets/Signs/.  
 
2. **Once this setup is complete, the generator will automatically**  
   a. Parse the road network  
   b. Place traffic signs following predefined rules  
   c. Deploy virtual cameras  
   d. Generate RGB images and segmentation masks using raycasting  

3. **Running the DatasetGenerator**  
   After configuring the scene:  
   a. Press Play.  
   b. Unity will simulate the environment and continuously generate annotated images into the specified output directory.

4. **Training the Detection Model**  
   The exported synthetic dataset can then be used directly with YOLOv8.  
   Annotation follows the standard YOLO format (class x_center y_center width height).  
   Training scripts and configs are included in the training/ folder.  

5. **3D Reconstruction and Semantic Integration**  
   Real-world UAV videos are processed via Metashape to reconstruct a dense point cloud.  
   The YOLOv8 predictions are then projected into the 3D structure to produce a semantic point cloud, followed by:  
   a. Class refinement  
   b. 3D clustering  
   c. Sign localization

## Notes
Due to GitHub's file size restrictions, Large files are hosted separately and will be accessible via a [cloud storage link](#) (to be updated).

## Licensing Notice
Due to the licensing restrictions of Unity and Sketchfab, the 3D models used in this project cannot be redistributed in this repository, as doing so would violate the terms described at:
https://unity.com/cn/legal/as-terms
To obtain the original 3D models, please download them directly from Sketchfab:
https://sketchfab.com/

## Contact
For questions or contributions, feel free to open an issue or contact the repository maintainers.
