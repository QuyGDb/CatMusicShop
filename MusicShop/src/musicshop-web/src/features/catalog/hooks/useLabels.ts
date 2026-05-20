import { createCrudHooks } from '@/shared/hooks/createCrudHooks';
import { labelService, CreateLabelRequest, UpdateLabelRequest } from '../services/labelService';
import { Label } from '../types';

const labelHooks = createCrudHooks<Label, CreateLabelRequest, UpdateLabelRequest>({
  queryKey: 'labels',
  service: {
    getAll: labelService.getLabels,
    getBySlug: labelService.getLabelBySlug,
    create: labelService.createLabel,
    update: labelService.updateLabel,
    delete: labelService.deleteLabel,
  },
  entityName: 'Label',
});

export const useLabels = labelHooks.useList;
export const useCreateLabel = labelHooks.useCreate;
export const useUpdateLabel = labelHooks.useUpdate;
export const useDeleteLabel = labelHooks.useDelete;
