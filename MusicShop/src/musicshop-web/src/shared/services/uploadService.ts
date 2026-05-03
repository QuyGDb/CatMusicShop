import axiosInstance from "@/shared/api/axiosInstance";
import { ApiResponse } from "@/shared/types/api";

export const uploadService = {
  uploadImage: async (file: File, folder: string = 'general'): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await axiosInstance.post<ApiResponse<string>>('/uploads/image', formData, {
      params: { folder },
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return response.data;
  },
};
