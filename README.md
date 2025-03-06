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
- **Traffic Sign Generation**: Scripts to identify roads in a virtual environment and generate traffic signs according to the defined workflow.
- **Camera Placement and Mask Generation**: Code to position cameras and generate mask images using raycasting.
- **Format Conversion Scripts**: Tools for converting data into compatible formats.
- **3D Semantic Point Cloud Processing**: Scripts for generating, classifying, and localizing traffic signs in a 3D semantic point cloud.

### Key Components
- **Traffic Sign Synthetic Dataset Generation**: The DatasetGenerator are written in **C#** and designed to run in a unreal engine. [Unity](https://unity.com/cn) was used in this study, specific steps are as follows: // C# script for DatasetGenerator
  - Import the city scene (Fig 2) from the [Unity Asset Store](https://assetstore.unity.com/?srsltid=AfmBOopX2Y6pVpbDR0U101dbc8TpX8v4A-gY8tA5f4f-Qa6QdKLMuj3K)
  - Import the provided DatasetGenerator scripts.
  - Tag road objects as `Road` in the city scene.
  Run the scripts to:
    - Identify roads and place traffic signs in a predefined order.
    - Position cameras and generate mask images of the camera's field of view using raycasting.
    - Output synthetic images with traffic sign annotations.

- **Model Training**:
  The generated synthetic dataset can be used to train the **YOLOv8** model. Refer to the official website: [ultralytics YOLO Vision](https://docs.ultralytics.com/zh).

- **Traffic Sign Localization**:
  Videos captured by a **DJI drone** are processed for traffic sign recognition.
  The 3D point cloud as shown in Fig 3 reconstructed from [Metashape](https://www.agisoft.com/) undergoes semantic segmentation to obtain precise geospatial coordinates of traffic signs.
    <table>
      <tr>
        <td align="center">
          <img src="city_scene.png" alt="City Scene" height="250">
          <br>
          Fig 2. Pre-constructed virtual city model
        </td>
        <td align="center">
          <img src="3D_point_cloud.png" alt="3D Point Cloud" height="250">
          <br>
          Fig 3. 3D point cloud reconstruction
        </td>
      </tr>
    </table>

### Detailed Workflow
For a step-by-step guide, refer to the accompanying paper. As the manuscript is currently under review, detailed information will be updated later.

## Notes
Due to GitHub's file size restrictions, Large files are hosted separately and will be accessible via a [cloud storage link](#) (to be updated).

## Contact
For questions or contributions, feel free to open an issue or contact the repository maintainers.
